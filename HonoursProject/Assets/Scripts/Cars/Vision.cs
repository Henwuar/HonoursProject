﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Car))]
[RequireComponent(typeof(EventSender))]
public class Vision : MonoBehaviour
{
    [SerializeField]
    private float visionDistance_;
    [SerializeField]
    private float stoppingDistance_;
    [SerializeField]
    private float lookAngle_;
    [SerializeField]
    private float lookStepAmount_;

    private Car car_;
    private Rigidbody body_;
    private EventSender eventSender_;

    private float curAngle_ = 0;
    private bool sweep_ = true;

	// Use this for initialization
	void Start ()
    {
        car_ = GetComponent<Car>();
        body_ = GetComponent<Rigidbody>();
        eventSender_ = GetComponent<EventSender>();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        //Raycast out to see if there is a car in front
        RaycastHit hit;
        //figure out the current view angle
        Vector3 lookDirection = Quaternion.Euler(new Vector3(0, curAngle_-(lookAngle_*0.5f))) * transform.forward;
        if(sweep_)
        {
            curAngle_ += lookStepAmount_;
            if (curAngle_ > lookAngle_)
            {
                curAngle_ = 0;
            }
        }

        Ray ray = new Ray(transform.position + transform.forward, (transform.forward + lookDirection.normalized).normalized);
        Debug.DrawLine(transform.position, transform.position + (transform.forward + lookDirection.normalized).normalized * visionDistance_, Color.blue);

        Physics.Raycast(ray, out hit, visionDistance_);
        //check that the ray hit something
        if(hit.collider && (hit.collider.tag == "Car" || hit.collider.tag == "Player"))
        {
            sweep_ = false;
            if (hit.distance < stoppingDistance_ && hit.distance > stoppingDistance_ * 0.5f)
            {
                //check if the car has just collided with another
                if(car_.FollowingTarget())
                {
                    //check if this car is moving faster than the other car
                    if(body_.velocity.magnitude > hit.collider.GetComponent<Rigidbody>().velocity.magnitude)
                    {
                        car_.Brake();
                    }
                    if(IsFacing(hit.collider.gameObject))
                    {
                        car_.Avoid(hit.collider.gameObject);
                    }
                }
                else
                {
                    car_.Accelerate(-1.0f);
                }
                //register an event happening with the event tracker
                eventSender_.SendEvent(TrafficEvent.TE_STOPPED_FOR_CAR);
            }
            else if(hit.distance < stoppingDistance_ * 0.5f)
            {
                car_.Wait(Time.deltaTime);
                car_.Brake();
            }
            else
            {
                car_.Accelerate(0);
            }
        }
        else if(eventSender_.CurEvent() == TrafficEvent.TE_STOPPED_FOR_CAR)
        {
            eventSender_.SendEvent(TrafficEvent.TE_NONE);
        }
        else
        {
            sweep_ = true;
        }
        if(hit.collider && hit.collider.tag == "TrafficLight")
        {
            //make sure the traffic light is facing the car
            if(IsFacing(hit.collider.gameObject))
            { 
                if (hit.collider.gameObject.GetComponent<TrafficLight>().GetSignal() == Signals.S_STOP && hit.distance < stoppingDistance_)
                {
                    car_.Wait(Time.deltaTime);
                    car_.Brake();
                    //register an event happening with the event tracker
                    eventSender_.SendEvent(TrafficEvent.TE_STOPPED_AT_LIGHT);
                }
                else if(eventSender_.CurEvent() == TrafficEvent.TE_STOPPED_AT_LIGHT)
                {
                    eventSender_.SendEvent(TrafficEvent.TE_NONE);
                }
            }
        }
	}

    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.collider.gameObject;
        if (other.tag == "Car")
        {
            //make sure only one car registers the event
            if (car_.DistanceToTarget() > other.GetComponent<Car>().DistanceToTarget())
            {
                eventSender_.SendEvent(TrafficEvent.TE_COLLISION);
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        GameObject other = collision.collider.gameObject;
        if(other.tag == "Car" || other.tag == "Player")
        {
            //head on collision
            if (IsFacing(other))
            {
                car_.Wait(1.0f);
                car_.Accelerate(-1.0f);
                car_.Avoid(other);
            }
            //rear end
            else
            {
                //check if this car is the rear ender
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

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Car")
        {
            print("Hit Car");
        }
    }

    bool IsFacing(GameObject other)
    {
        float angle = Vector3.Angle(transform.forward, other.transform.forward);
        if (angle > 90 && angle < 270)
        {
            return true;
        }
        return false;
    }

}
