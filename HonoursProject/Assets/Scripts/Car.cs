using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{
    [SerializeField]
    private float acceleration_;
    [SerializeField]
    private float brakePower_;
    [SerializeField]
    private float maxSpeed_;
    [SerializeField]
    private float turnSpeed_;

    private Rigidbody body_;

	// Use this for initialization
	void Start ()
    {
        body_ = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.red);
        Debug.DrawRay(transform.position, body_.velocity, Color.blue);

        Steer();

        //process the input
        if (Input.GetAxis("Accelerate") > 0)
        {
            Accelerate();
        }
        else if(Input.GetAxis("Brake") > 0)
        {
            Brake();
        }
	}

    //rotates the vehicle based on input
    public void Steer()
    {
        if(body_.velocity.magnitude > 0.1f || body_.velocity.magnitude < -0.1f)
        {
            float dir = 1;
            if(Vector3.Angle(transform.forward, body_.velocity) > 0)
            {
                dir = -1;
            }
            float steering = Input.GetAxis("Horizontal") * turnSpeed_ * dir;
            transform.Rotate(new Vector3(0, steering, 0));
        }
        
    }

    //accelerates the vehicle along its forward vector
    public void Accelerate()
    {
        //apply the acceleration
        body_.AddForce(transform.forward * acceleration_);
        //make sure the velocity doesn't exceed the maximum
        body_.velocity = Vector3.ClampMagnitude(body_.velocity, maxSpeed_);
    }

    //decelerates the vehicle
    public void Brake()
    {
        //apply the force
        body_.AddForce(transform.forward * -brakePower_);
        //clamp the vehicles reversing speed
        if(Vector3.Angle(body_.velocity, transform.forward) > 0)
        {
            body_.velocity = Vector3.ClampMagnitude(body_.velocity, maxSpeed_ * 0.25f);
        }
    }
}
