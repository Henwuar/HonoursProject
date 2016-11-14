using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Car))]
public class Vision : MonoBehaviour
{
    [SerializeField]
    private float visionDistance_;
    [SerializeField]
    private float stoppingDistance_;

    private Car car_;
    private Rigidbody body_;

	// Use this for initialization
	void Start ()
    {
        car_ = GetComponent<Car>();
        body_ = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        //Raycast out to see if there is a car in front
        RaycastHit hit;
        Ray ray = new Ray(transform.position + transform.forward, transform.forward);

        Debug.DrawRay(transform.position + transform.forward, transform.forward * visionDistance_, Color.green);

        Physics.Raycast(ray, out hit, visionDistance_);
        //check that the ray hit something
        if(hit.collider && (hit.collider.tag == "Car" || hit.collider.tag == "Player"))
        {

            if(hit.distance < stoppingDistance_)
            {
                //check if the car has just collided with another
                if(car_.followingTarget_)
                {
                    car_.Brake();
                }
                else
                {
                    car_.Accelerate(-1.0f);
                }
            }
            else
            {
                car_.Accelerate(0);
            }
        }
	}

    void OnCollisionStay(Collision collision)
    {
        GameObject other = collision.collider.gameObject;
        if(other.tag == "Car" || other.tag == "Player")
        {
            //get the type of collision
            float angle = Vector3.Angle(transform.forward, other.transform.forward);
            //head on collision
            if (angle > 90 && angle < 270)
            {
                print("reversing");
                car_.Wait(1.0f);
                car_.Accelerate(-1.0f);
            }
            //rear end
            else
            {
                //check if this car is the rear ender or not
                car_.Wait(1);
                if(Vector3.Cross(transform.right, other.transform.position - transform.position).y < 0)
                {
                    car_.Accelerate(-1.0f);
                }

            }
            
        }
    }

    void OnCollisionExit(Collision collision)
    {
        GameObject other = collision.collider.gameObject;
        if (other.tag == "Car")
        {
            if (car_.DistanceToTarget() > other.GetComponent<Car>().DistanceToTarget())
            {
                car_.Wait(2);
            }
        }
    }
}
