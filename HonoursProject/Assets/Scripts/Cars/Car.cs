using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CarState { CS_MOVING, CS_STOPPING, CS_STALLED, CS_STARTING, CS_PARKING, CS_PARKED, CS_DEPARKING}

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{
    [SerializeField]
    private bool controlled_;
    [SerializeField]
    private float acceleration_;
    [SerializeField]
    private float brakePower_;
    [SerializeField]
    private float maxSpeed_;
    [SerializeField]
    private float turnSpeed_;
    [SerializeField]
    private float arrivalDistance_;
    [SerializeField]
    private float fineMovementMultiplier_;
    [SerializeField]
    private float wheelRotationSpeed_;
    [SerializeField]
    private float brakeLightIntensity_;
    [SerializeField]
    private float headLightIntensity_;
    [SerializeField]
    private Vector2 reverseAngleLimits_;

    private Rigidbody body_;
    private Vector3 target_;
    private Vision vision_ = null;
    private Error error_ = null;
    private Purpose purpose_ = null;
    private GameObject avoidTarget_;
    private Queue<Vector3> targets_;
    private GameObject curRoad_;
    private int gear_;
    private Vector3 respawnPoint_;
    private bool initialised_ = false;
    private float waitTime_;
    private bool followingTarget_ = true;
    private bool stopping_ = false;
    [SerializeField]
    private CarState state_ = CarState.CS_MOVING;
    private CarState prevState_ = CarState.CS_MOVING;
    private float curLightIntensity_;

    private int[] statePriorities = {0, 0, 1, 1, 2, 2, 3};

	// Use this for initialization
	void Start ()
    {
        body_ = GetComponent<Rigidbody>();
        gear_ = 0;
        respawnPoint_ = transform.position;
        //initialised_ = false;
        curLightIntensity_ = brakeLightIntensity_ * 0.5f;
        error_ = GetComponent<Error>();
        purpose_ = GetComponent<Purpose>();
        vision_ = GetComponent<Vision>();
    }

    public void Init()
    {
        initialised_ = true;
        targets_ = new Queue<Vector3>();
        //find all the available roads
        GameObject[] roads = GameObject.FindGameObjectsWithTag("Road");
        GameObject closestRoad = roads[0];
        //find the closest road
        for(int i = 0; i < roads.GetLength(0); i++)
        {
            //GameObject road = roads[i];
            float dist = Vector3.Distance(roads[i].transform.position, transform.position);
            if(dist < Vector3.Distance(closestRoad.transform.position, transform.position))
            {
                closestRoad = roads[i];
            }
        }
        //chose a target road from them
        //int chosenRoad = Random.Range(0, roads.GetLength(0));
        target_ = closestRoad.transform.position;
        curRoad_ = closestRoad.transform.parent.gameObject;
        targets_.Enqueue(curRoad_.GetComponent<Road>().GetEnd().position);
        transform.LookAt(target_);
    }

	// Update is called once per frame
	void Update ()
    {
        //make sure the car is initialised
        if (!initialised_)
        {
            return;
        }

        //reset the current light intensity
        curLightIntensity_ = brakeLightIntensity_ * 0.5f;
        //clear the avoid target
        avoidTarget_ = null;


        //update the components based on state
        if (state_ == CarState.CS_PARKED)
        {
            if (vision_)
            {
                vision_.enabled = false;
            }

        }
        else
        {
            if (vision_)
            {
                vision_.enabled = true;
            }
        }

        //steer towards the target
        Steer();
        
        //make sure the car isn't waiting
        if (waitTime_ > 0)
        {
            waitTime_ -= Time.deltaTime;
            if(waitTime_ < 0)
            {
                waitTime_ = 0;
                followingTarget_ = true;
            }
            return;
        }        

        if (controlled_)
        {
            HandleInput();
        }
        else if(followingTarget_)
        {
            MoveToTarget();
        }

        //make the respawn point follow the player
        respawnPoint_ = new Vector3(transform.position.x, respawnPoint_.y, transform.position.z);

        //update the tag of the car
        if (controlled_)
        {
            tag = "Player";
            GetComponent<Vision>().enabled = false;
        }
        else
        {
            tag = "Car";
            GetComponent<Vision>().enabled = true;
        }

        //resetting
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.up, out hit);
        if (hit.collider && hit.collider.gameObject.tag == "Ground")
        {
            Reset();
        }

        UpdateLights();

        //print(curRoad_);

        Debug.DrawLine(transform.position, target_, Color.green);
        //Debug.DrawLine(target_.position + Vector3.right, target_.position - Vector3.right, Color.blue);
        //Debug.DrawLine(target_.position + Vector3.forward, target_.position - Vector3.forward, Color.blue);
    }

    //handles response to input
    void HandleInput()
    {
        //process the input
        //acceleration
        if (Input.GetAxis("Accelerate") > 0)
        {
            if(gear_ == -1)
            {
                Brake();
            }
            gear_ = 1;
            Accelerate(Input.GetAxis("Accelerate")*2);
        }
        else if (Input.GetAxis("Brake") <= 0) 
        {
            gear_ = 0;
            Accelerate(0);
        }
        //braking
        if (Input.GetAxis("Brake") > 0)
        {
            //brake or reverse
            if (body_.velocity.magnitude > 0.1f && gear_ >= 0)
            {
                Brake();
            }
            else if(gear_ >= 0)
            {
                gear_ = -1;
            }
            if(gear_ == -1)
            {
                Accelerate(-Input.GetAxis("Brake") * 0.5f);
            }
        }
    }

    void UpdateLights()
    {
        if(state_ == CarState.CS_STALLED || state_ == CarState.CS_PARKED)
        {
            Light[] lights = GetComponentsInChildren<Light>();
            foreach (Light light in lights)
            {
                light.intensity = 0;
            }
        }
        else if (state_ == CarState.CS_STARTING)
        {
            float intensity = Random.Range(brakeLightIntensity_ * 0.5f, 2.0f * brakeLightIntensity_);
            Light[] tailLights = transform.FindChild("Taillights").GetComponentsInChildren<Light>();
            foreach (Light light in tailLights)
            {
                light.intensity = intensity;
            }
            intensity = Random.Range(headLightIntensity_ * 0.5f, 2.0f * headLightIntensity_);
            Light[] headLights = transform.FindChild("Headlights").GetComponentsInChildren<Light>();
            foreach (Light light in headLights)
            {
                light.intensity = intensity;
            }
        }
        else
        {
            //print(curLightIntensity_);
            Light[] tailLights = transform.FindChild("Taillights").GetComponentsInChildren<Light>();
            foreach (Light light in tailLights)
            {
                light.intensity = curLightIntensity_;
            }
            Light[] headLights = transform.FindChild("Headlights").GetComponentsInChildren<Light>();
            foreach (Light light in headLights)
            {
                light.intensity = headLightIntensity_;
            }
        }
    }

    //rotates the vehicle based on input
    public void Steer()
    {
        //get the wheel colliders
        WheelCollider left = transform.FindChild("wheel_FL").gameObject.GetComponent<WheelCollider>();
        WheelCollider right = transform.FindChild("wheel_FR").gameObject.GetComponent<WheelCollider>();

        float angle;

        if (controlled_)
        {
            angle = Input.GetAxis("Horizontal") * turnSpeed_;
        }
        else
        {
            Vector3 targetPos = new Vector3(target_.x, transform.position.y, target_.z);
            if(avoidTarget_)
            {
                //move the target to the right of the avoid target
                targetPos = avoidTarget_.transform.position - Vector3.Cross(Vector3.up, avoidTarget_.transform.position - transform.position);
            }
            //get the abosulte angle to the point
            angle = Vector3.Angle(transform.forward, targetPos - transform.position);
            angle = Mathf.Abs(angle);

            //clamp the angle correcting for reversing
            if(angle > reverseAngleLimits_.x && angle < reverseAngleLimits_.y)
            {
                angle -= 180;
                angle *= -1;
            }
            else if (Mathf.Abs(angle) > turnSpeed_)
            {
                angle = turnSpeed_;
            }
            //get the right direction
            angle *= Mathf.Sign(Vector3.Cross(transform.forward, targetPos - transform.position).y);
            
        }

        //steer the colliders
        left.steerAngle = Mathf.Lerp(left.steerAngle, angle, wheelRotationSpeed_ * Time.deltaTime);
        right.steerAngle = Mathf.Lerp(right.steerAngle, angle, wheelRotationSpeed_ * Time.deltaTime);

        //rotate the meshes
        left.gameObject.transform.localRotation = Quaternion.Euler(0, left.steerAngle, 0);
        right.gameObject.transform.localRotation = Quaternion.Euler(0, right.steerAngle, 0);
    }

    //accelerates the vehicle along its forward vector
    public void Accelerate(float amount)
    {
        if(state_ == CarState.CS_MOVING || state_ == CarState.CS_PARKING)
        {
            if(error_ && amount != 0 && !controlled_)
            {
                error_.TestStall();
            }
            float power = amount * acceleration_ * Time.deltaTime;
            //add torque to the wheels
            WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>();
            foreach (WheelCollider wheel in wheels)
            {
                wheel.motorTorque = power;
                wheel.brakeTorque = 0.0f;
            }
        }
    }

    //decelerates the vehicle
    public void Brake(float amount = 1)
    {
        //apply the brake lights
        curLightIntensity_ = brakeLightIntensity_;

        //apply torque to the wheels
        WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider wheel in wheels)
        {
            wheel.brakeTorque = amount * body_.mass * brakePower_ * body_.velocity.magnitude;
            wheel.motorTorque = 0.0f;
        }
    }

    public void Reset()
    {
        //reset the transform
        transform.position = respawnPoint_;
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        body_.velocity = Vector3.zero;
        body_.angularVelocity = Vector3.zero;
        //stop the wheels
        Brake();
    }

    public void MoveToTarget()
    {
        if(error_ && error_.GetDistracted())
        {
            //check if the body is stopped (account for slight drift)
            if(body_.velocity.magnitude < 0.01f)
            {
                return;
            }
        }

        //apply the fine movement modifier
        float distanceMultiplier = state_ == CarState.CS_PARKING ? fineMovementMultiplier_ : 1.0f;
     
        if (DistanceToTarget() > arrivalDistance_ * distanceMultiplier)
        {
            //make sure the car doesn't exceed the maximum speed
            if (body_.velocity.magnitude < maxSpeed_ * distanceMultiplier)
            {
                float steerAngle = Mathf.Abs(transform.FindChild("wheel_FL").GetComponent<WheelCollider>().steerAngle);
                float targetAngle = AngleToTarget();
                float direction = 1.0f;
                if (targetAngle > reverseAngleLimits_.x && targetAngle < reverseAngleLimits_.y && body_.velocity.magnitude < maxSpeed_ * 0.25f)
                {
                    print("reversing");
                    direction *= -1.0f;
                }
                Accelerate(Mathf.Min(1/(steerAngle / turnSpeed_), 2.5f) * direction);
            }
            else
            {
                Accelerate(0);
            }
        }
        else
        {
            Brake();
            //if slowed enough, find a new target
            if (body_.velocity.magnitude < maxSpeed_ * 0.75f * distanceMultiplier)
            {
                //make sure there are target
                if (targets_.Count == 0)
                {
                    if(state_ == CarState.CS_PARKING)
                    {
                        state_ = CarState.CS_PARKED;
                        return;
                    }
                    else
                    {
                        GetTargets();
                    }
                    
                }
                else if (purpose_ && state_ != CarState.CS_PARKING)
                {
                    purpose_.TestPark();
                }
                //get a new target from the queue
                target_ = targets_.Dequeue();
                
            }
        }
    }

    public void Wait(float time, bool additive = false)
    {
        if(followingTarget_)
        {
            if (additive)
            {
                waitTime_ += time;
            }
            else
            {
                waitTime_ = time;
            }
            followingTarget_ = false;
        }
        
    }

    public float DistanceToTarget()
    {
        return Vector3.Distance(transform.position, target_);
    }

    float AngleToTarget()
    {
        Vector3 targetPos = new Vector3(target_.x, transform.position.y, target_.z);
        return Vector3.Angle(transform.forward, targetPos - transform.position);
    }

    void GetTargets()
    {
        
        curRoad_ = curRoad_.GetComponent<Road>().GetJunction().GetNewRoad(curRoad_);
        Road newRoad = curRoad_.GetComponent<Road>();
        targets_.Enqueue(newRoad.GetStart().position);
        targets_.Enqueue(newRoad.GetEnd().position);
    }

    public bool FollowingTarget()
    {
        return followingTarget_;
    }

    public void Avoid(GameObject other)
    {
        avoidTarget_ = other;
    }

    public void ToggleControlled()
    {
        controlled_ = !controlled_;
    }

    public void SetStopping(bool value)
    {
        stopping_ = value;
    }

    public void SetState(CarState newState, bool overridePriority = false)
    {
        //check if the new state can be set based on its priority
        if (statePriorities[(int)newState] >= statePriorities[(int)state_] || overridePriority)
        {
            state_ = newState;
        }
    }

    public void RevertState()
    {
        
    }

    public CarState GetState()
    {
        return state_;
    }

    public Vector3 GetCurTarget()
    {
        return target_;
    }

    public void SetTarget(Vector3 newTarget)
    {
        target_ = newTarget;
    }

    public void AddTarget(Vector3 target)
    {
        targets_.Enqueue(target);
    }

    public void ClearTargets()
    {
        targets_.Clear();
    }

    public GameObject GetCurRoad()
    {
        return curRoad_;
    }

    public float GetFineMovementMutliplier()
    {
        return fineMovementMultiplier_;
    }
}
