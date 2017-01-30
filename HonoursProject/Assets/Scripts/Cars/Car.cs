using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CarState { CS_MOVING, CS_STOPPING, CS_STALLED, CS_STARTING, CS_PARKING}

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
    private float wheelRotationSpeed_;
    [SerializeField]
    private float brakeLightIntensity_;
    [SerializeField]
    private float headLightIntensity_;

    private Rigidbody body_;
    private Transform target_;
    private Error error_ = null;
    private Purpose purpose_ = null;
    private GameObject avoidTarget_;
    private Queue<Transform> targets_;
    private int gear_;
    private Vector3 respawnPoint_;
    private bool initialised_ = false;
    private float waitTime_;
    private bool followingTarget_ = true;
    private bool stopping_ = false;
    private CarState state_ = CarState.CS_MOVING;
    private CarState prevState_ = CarState.CS_MOVING;
    private float curLightIntensity_;

    private int[] statePriorities = {0, 0, 1, 1, 2};

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
    }

    public void Init()
    {
        initialised_ = true;
        targets_ = new Queue<Transform>();
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
        target_ = closestRoad.transform;
        targets_.Enqueue(target_.GetComponentInParent<Road>().GetEnd());
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
        
        Debug.DrawLine(transform.position, target_.position, Color.blue);
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
        if(state_ == CarState.CS_STALLED)
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
            Vector3 targetPos = new Vector3(target_.position.x, transform.position.y, target_.position.z);
            if(avoidTarget_)
            {
                //move the target to the right of the avoid target
                targetPos = avoidTarget_.transform.position - Vector3.Cross(Vector3.up, avoidTarget_.transform.position - transform.position);
            }
            //get the abosulte angle to the point
            angle = Vector3.Angle(transform.forward, targetPos - transform.position);
            //clamp it at the turning speed;
            if(Mathf.Abs(angle) > turnSpeed_)
            {
                //Brake(0.1f);
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
        if (DistanceToTarget() > arrivalDistance_)
        {
            //make sure the car doesn't exceed the maximum speed
            if (body_.velocity.magnitude < maxSpeed_)
            {
                float angle = Mathf.Abs(transform.FindChild("wheel_FL").GetComponent<WheelCollider>().steerAngle);
                Accelerate(Mathf.Min(1/(angle/turnSpeed_), 2.5f));
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
            if (body_.velocity.magnitude < maxSpeed_ * 0.75f)
            {
                //make sure there are target
                if (targets_.Count == 0)
                {
                    GetTargets();
                }
                else if (purpose_)
                {
                    purpose_.TestPark();
                }
                if (state_ != CarState.CS_PARKING)
                {
                    //get a new target from the queue
                    target_ = targets_.Dequeue();
                }
                /*else
                {
                    Wait(purpose_.GetStopTime());
                }*/
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
        return Vector3.Distance(transform.position, target_.position);
    }

    void GetTargets()
    {
        List<Transform> newTargets = target_.gameObject.GetComponentInParent<Road>().GetJunction().GetNewTargets();
        //check if a valid target has been provided
        bool canContinue = false;
        while(!canContinue)
        {
            if(newTargets.Count > 1)
            {
                Vector3 newDir = newTargets[1].position - newTargets[0].position;
                float angle = Vector3.Angle(transform.forward, newDir);
                //invalid - find new targets
                if (angle > 135 && angle < 225)
                {
                    newTargets.Clear();
                    newTargets = target_.gameObject.GetComponentInParent<Road>().GetJunction().GetNewTargets();
                }
                else
                {
                    canContinue = true;
                }
            }
            else //not enough values to check - continue
            {
                canContinue = true;
            }
        }
        foreach (Transform target in newTargets)
        {
            targets_.Enqueue(target);
        }
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

    public Transform GetCurTarget()
    {
        return target_;
    }

    public void SetTarget(Transform newTarget)
    {
        target_ = newTarget;
    }
}
