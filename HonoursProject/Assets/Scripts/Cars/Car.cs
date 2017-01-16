using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private float stopDistance_;

    private Rigidbody body_;
    private Transform target_;
    private GameObject avoidTarget_;
    private Queue<Transform> targets_;
    private int gear_;
    private Vector3 respawnPoint_;
    private bool initialised_ = false;
    private float waitTime_;
    private bool followingTarget_ = true;

	// Use this for initialization
	void Start ()
    {
        body_ = GetComponent<Rigidbody>();
        gear_ = 0;
        respawnPoint_ = transform.position;
        //initialised_ = false;
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
        targets_.Enqueue(target_.GetComponentInParent<Road>().end_.transform);
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
            print("Reset");
            Reset();
        }


        
        //Debug.DrawLine(transform.position, target_.position, Color.blue);
        //Debug.DrawLine(target_.position + Vector3.right, target_.position - Vector3.right, Color.blue);
        //Debug.DrawLine(target_.position + Vector3.forward, target_.position - Vector3.forward, Color.blue);
    }

    void LateUpdate()
    {
        //clear the avoid target
        avoidTarget_ = null;
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
            Vector3 targetPos_ = target_.position;
            if(avoidTarget_)
            {
                //move the target to the left of the avoid target
                targetPos_ = avoidTarget_.transform.position + Vector3.Cross(Vector3.up, avoidTarget_.transform.position - transform.position);
            }
            //get the abosulte angle to the point
            angle = Vector3.Angle(transform.forward, targetPos_ - transform.position);
            //clamp it at the turning speed;
            if(angle > turnSpeed_)
            {
                //Brake();
                angle = turnSpeed_;
            }
            //get the right direction
            angle *= Mathf.Sign(Vector3.Cross(transform.forward, targetPos_ - transform.position).y);

            //Debug.DrawLine(transform.position, targetPos_, Color.blue);
        }

        //steer the colliders
        left.steerAngle = angle;
        right.steerAngle = angle;

        //rotate the meshes
        left.gameObject.transform.localRotation = Quaternion.Euler(0, left.steerAngle, 0);
        right.gameObject.transform.localRotation = Quaternion.Euler(0, right.steerAngle, 0);
    }

    //accelerates the vehicle along its forward vector
    public void Accelerate(float direction)
    {
        float power = direction * acceleration_ * Time.deltaTime;
        //add torque to the wheels
        WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider wheel in wheels)
        {
            wheel.motorTorque = power;
            wheel.brakeTorque = 0.0f;
        }
    }

    //decelerates the vehicle
    public void Brake()
    {
        //add torque to the wheels
        WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider wheel in wheels)
        {
            wheel.brakeTorque = body_.mass * brakePower_ * 0.1f;
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
        if (DistanceToTarget() > stopDistance_)
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
            //if stopped, find a new target
            if(body_.velocity.magnitude < maxSpeed_*0.75f)
            {
                //make sure there are target
                if(targets_.Count == 0)
                {
                    GetTargets();
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
        return Vector3.Distance(transform.position, target_.position);
    }

    void GetTargets()
    {
        List<Transform> newTargets = target_.gameObject.GetComponentInParent<Road>().GetJunction().GetNewTargets();
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
}
