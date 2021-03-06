﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Car))]
[RequireComponent(typeof(EventSender))]
public class Vision : MonoBehaviour
{
    [SerializeField]
    private float visionDistance_;
    [SerializeField]
    private float lookAheadMultiplier_;
    [SerializeField]
    private float stoppingDistance_;
    [SerializeField]
    private float lookAngle_;
    [SerializeField]
    private float lookStepAmount_;
    [SerializeField]
    private AudioClip crashNoise_;

    private Car car_;
    private Error error_ = null;
    private Purpose purpose_ = null;
    private Rigidbody body_;
    private EventSender eventSender_;

    private float curAngle_ = 0;
    private bool sweep_ = true;
    private float startAngle_ = 0;
    private bool prevObjectAhead_ = false;
    private bool objectAhead_ = false;

	// Use this for initialization
	void Start ()
    {
        car_ = GetComponent<Car>();
        body_ = GetComponent<Rigidbody>();
        eventSender_ = GetComponent<EventSender>();
        error_ = GetComponent<Error>();
        purpose_ = GetComponent<Purpose>();
	}
	
    void OnAwake()
    {
        Start();
    }

	// Update is called once per frame
	void FixedUpdate ()
    {
        //Raycast out to see if there is a car in front
        RaycastHit hit;
        //figure out the current view angle
        Vector3 lookDirection = Quaternion.Euler(new Vector3(0, startAngle_ + curAngle_-(lookAngle_*0.5f))) * transform.forward;
        if(error_ && error_.GetDistracted())
        {
            sweep_ = false;
        }

        if(sweep_)
        {
            curAngle_ += lookStepAmount_;
            if (curAngle_ > lookAngle_)
            {
                curAngle_ = 0;
            }
        }

        Ray ray = new Ray(transform.position + transform.forward, (transform.forward + lookDirection.normalized).normalized);

        float lookAhead = Mathf.Clamp(body_.velocity.magnitude, 1, lookAheadMultiplier_);
        Debug.DrawLine(transform.position, transform.position + (transform.forward + lookDirection.normalized).normalized * visionDistance_ * lookAhead, Color.blue);

        objectAhead_ = Physics.Raycast(ray, out hit, visionDistance_*lookAhead);
        if(objectAhead_ && hit.collider.tag == "TrafficLight")
        {
            objectAhead_ = false;
        }

        bool stopping = car_.GetState() == CarState.CS_PARKING || car_.GetState() == CarState.CS_DEPARKING;
        float stoppingMultiplier = stopping ? car_.GetFineMovementMutliplier() : 1.0f;

        //check that the ray hit something
        if(hit.collider && (hit.collider.tag == "Car" || hit.collider.tag == "Player"))
        {
            //check if there's a car coming towards them at speed
            if (hit.rigidbody && hit.rigidbody.velocity.magnitude > car_.GetMaxSpeed() * 0.25f)
            {
                if (IsFacing(hit.collider.gameObject))
                {
                    if (purpose_.enabled)
                    {
                        purpose_.TestImpatience();//SoundHorn();
                    }
                }
            }

            if (hit.distance < stoppingDistance_ * stoppingMultiplier && hit.distance > stoppingDistance_ * stoppingMultiplier * 0.5f)
            {
                car_.SetState(CarState.CS_STOPPING);
                //check if the car has just collided with another
                if (car_.FollowingTarget())
                {
                    //check if this car is moving faster than the other car
                    if(body_.velocity.magnitude > hit.collider.GetComponent<Rigidbody>().velocity.magnitude)
                    {
                        car_.Brake(hit.distance / stoppingDistance_);
                    }
                    if(IsFacing(hit.collider.gameObject))
                    {
                        car_.Avoid(hit.collider.gameObject);
                    }
                }
                else
                {
                    car_.Accelerate(-0.5f);
                }
            }
            else if(hit.distance < stoppingDistance_ * stoppingMultiplier * 0.5f)
            {
                //stop following targets for the next frame
                car_.Wait(Time.deltaTime);
                car_.Brake();
            }
            else
            {
                car_.SetState(CarState.CS_MOVING);
                car_.Accelerate(0);
            }
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
                    car_.Brake(hit.distance/stoppingDistance_);
                    car_.SetState(CarState.CS_STOPPING);
                }
                if(hit.collider.gameObject.GetComponent<TrafficLight>().GetSignal() == Signals.S_GO)
                {
                    car_.SetState(CarState.CS_MOVING);
                }
            }
        }
        //check if sweeping should be stopped
        if(hit.collider && ((hit.collider.tag == "Car" || hit.collider.tag == "Player") || (hit.collider.tag == "TrafficLight" &&  IsFacing(hit.collider.gameObject))))
        {
            sweep_ = false;
        }
        else
        {
            sweep_ = true;
            car_.SetState(CarState.CS_MOVING);
        }

        if(!objectAhead_)
        {
            if(prevObjectAhead_)
            {
                if(error_ && error_.enabled)
                {
                    car_.Wait(error_.GetReactionTime());
                }
            }
        }
        prevObjectAhead_ = objectAhead_;
	}

    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.collider.gameObject;
        if (other.tag != "Ground")
        {
            GetComponent<AudioSource>().PlayOneShot(crashNoise_);
        }

        if(purpose_ && purpose_.enabled)
        {
            purpose_.SoundHorn();
        }

        if(tag == "Player")
        {
            return;
        }
        if (collision.relativeVelocity.magnitude > car_.GetMaxSpeed() * 0.25f)
        {
            car_.SetState(CarState.CS_CRASHED);
        }
        else
        {
            if(purpose_.enabled)
            {
                car_.MoveAwayFrom(other);
                purpose_.SetParkingChance(101.0f);
            }
        }
        if (other.tag == "Car" || other.tag == "Player")
        {
            //make sure only one car registers the event
            if (car_.DistanceToTarget() > other.GetComponent<Car>().DistanceToTarget())
            {
                eventSender_.SendEvent(TrafficEvent.TE_COLLISION);
            }
            if(error_)
            {
                car_.SetState(CarState.CS_MOVING, true);
            }
        }

        //prevent the car from locking up
        if(tag == "Player")
        {
            car_.Brake(0);
            car_.Accelerate(0);
            body_.velocity *= 0.1f;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (tag == "Player")
        {
            return;
        }
        GameObject other = collision.collider.gameObject;
        if((other.tag == "Car" || other.tag == "Player") && car_.GetState() != CarState.CS_CRASHED)
        {
            car_.SetState(CarState.CS_MOVING);
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
                else
                {
                    car_.Accelerate(1.0f);
                }

            }
            
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (tag == "Player")
        {
            return;
        }
        GameObject other = collision.collider.gameObject;
        if (other.tag == "Car")
        {
            if (car_.DistanceToTarget() > other.GetComponent<Car>().DistanceToTarget())
            {
                car_.Wait(Time.deltaTime*2);
            }
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

    public void SetLookStartAngle(float value)
    {
        startAngle_ = value;
    }

    public float GetLookAngle()
    {
        return lookAngle_;
    }

    public float GetStoppingDistance()
    {
        return stoppingDistance_;
    }

    public void SetStoppingDistance(float value)
    {
        stoppingDistance_ = value;
    }

    public float GetVisionDistance()
    {
        return visionDistance_;
    }

    public void SetVisionDistance(float value)
    {
        visionDistance_ = value;
    }

    public void SetLookAngle(float value)
    {
        lookAngle_ = value;
    }

    public bool ObjectAhead()
    {
        return objectAhead_;
    }
}
