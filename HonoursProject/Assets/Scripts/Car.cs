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
    private Vector3 target_;
    private int gear_;

	// Use this for initialization
	void Start ()
    {
        body_ = GetComponent<Rigidbody>();
        gear_ = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.red);
        Debug.DrawRay(transform.position, body_.velocity, Color.blue);

        Steer();

        if(controlled_)
        {
            UpdateInput();
        }
        
	}

    void UpdateInput()
    {
        print(gear_);
        //process the input
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
        transform.FindChild("wheel_FL").gameObject.GetComponent<WheelCollider>().steerAngle = Input.GetAxis("Horizontal") * turnSpeed_;
        transform.FindChild("wheel_FR").gameObject.GetComponent<WheelCollider>().steerAngle = Input.GetAxis("Horizontal") * turnSpeed_;
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
}
