using UnityEngine;
using System.Collections;

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

    private Rigidbody body_;
    private Transform target_;
    private int gear_;
    private Vector3 respawnPoint_;

	// Use this for initialization
	void Start ()
    {
        body_ = GetComponent<Rigidbody>();
        gear_ = 0;
        respawnPoint_ = transform.position;
        target_ = GameObject.FindGameObjectWithTag("Junction").transform;
	}
	
	// Update is called once per frame
	void Update ()
    {

        Steer();

        if(controlled_)
        {
            HandleInput();
        }
        else
        {
            MoveToTarget();
        }
        //make the respawn point follow the player
        respawnPoint_ = new Vector3(transform.position.x, respawnPoint_.y, transform.position.z);

        //update the tag of the car
        if (controlled_)
        {
            tag = "Player";
        }
        else
        {
            tag = "Car";
        }
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
            Accelerate(Input.GetAxis("Accelerate"));
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
        //resetting
        if(Input.GetButtonDown("Fire1"))
        {
            Reset();
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
            //get the abosulte angle to the point
            angle = Vector3.Angle(transform.forward, target_.position - transform.position);
            //clamp it at the turning speed;
            if(angle > turnSpeed_)
            {
                Brake();
                angle = turnSpeed_;
            }
            print(angle);
            //get the right direction
            angle *= Mathf.Sign(Vector3.Cross(transform.forward, target_.position - transform.position).y);

            Debug.DrawRay(transform.position, Quaternion.AngleAxis(angle, Vector3.up) * (transform.forward), Color.green);
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
        //stop the wheels
        Brake();
    }

    //moves towards a junction
    public void MoveToTarget()
    {
        if(Vector3.Distance(transform.position, target_.position) > 5.0f)
        {
            if(body_.velocity.magnitude < maxSpeed_)
            {
                Accelerate(1);
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
            if(body_.velocity.magnitude < 0.1f)
            {
                target_ = target_.gameObject.GetComponent<Junction>().GetJunction().transform;
            }
        }
    }
}
