using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Car))]
public class Purpose : MonoBehaviour
{
    [SerializeField]
    private float parkChance_;
    [SerializeField]
    private float parkingTime_;
    [SerializeField]
    private float baseImpatience_;
    [SerializeField]
    AudioClip[] hornNoises_;
    [SerializeField]
    private float impatienceIncSpeed_;
    [SerializeField]
    private float parkedAngle_;

    private Car car_;
    private ParkingSpace curParking_ = null;
    private AudioSource hornSource_;
    private Rigidbody body_;
    private Vision vision_ = null;

    private float parkTimer_;
    private float impatience_;

	// Use this for initialization
	void Start ()
    {
        car_ = GetComponent<Car>();
        impatience_ = baseImpatience_;
        hornSource_ = transform.Find("Horn").GetComponent<AudioSource>();
        body_ = GetComponent<Rigidbody>();
        vision_ = GetComponent<Vision>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(car_.GetState() == CarState.CS_PARKED)
        {
            parkTimer_ += Time.deltaTime;
            if(parkTimer_ > parkingTime_)
            {
                car_.SetState(CarState.CS_DEPARKING);
                car_.ClearTargets();
                List<Vector3> newTargets = curParking_.GetExitTargets();
                foreach (Vector3 target in newTargets)
                {
                    car_.AddTarget(target);
                }
                parkTimer_ = 0;
            }
        }
        else if(car_.GetState() == CarState.CS_MOVING)
        {
            if(curParking_)
            {
                curParking_.SetAvailable(false);
                curParking_ = null;
            }
        }

        //check if the car has stopped
        if(body_.velocity.magnitude < 0.1f)
        {
            //check if the car should be going
            if(car_.GetCurRoad().GetComponent<Road>().GetEnd().Find("TrafficLight").GetComponent<TrafficLight>().GetSignal() == Signals.S_GO)
            {
                if(vision_)
                {
                    //make sure the car isn't honking at itself
                    if(vision_.ObjectAhead())
                    {
                        TestImpatience();
                    }
                }
            }
        }

        if(impatience_ > baseImpatience_)
        {
            impatience_ -= impatienceIncSpeed_ * Time.deltaTime;
        }
        else
        {
            impatience_ = baseImpatience_;
        }
	}

    public void TestPark()
    {
        if(Random.Range(0.0f, 100.0f) < parkChance_)
        {
            Road road = car_.GetCurRoad().GetComponent<Road>();
            ParkingSpace parking = null;
            if(road)
            {
                GameObject space = road.GetParkingSpace();
                if(space)
                {
                    parking = space.GetComponent<ParkingSpace>();
                }
            }
            
            if (parking)
            {
                car_.SetState(CarState.CS_PARKING);
                parking.SetAvailable(false);
                
                car_.ClearTargets();
                List<Vector3> newTargets = parking.GetEnterTargets();
                foreach(Vector3 target in newTargets)
                {
                    car_.AddTarget(target);
                }

                curParking_ = parking;
            }
        }   
    }

    public void TestImpatience()
    {
        //test if the car is going to honk
        if (Random.Range(0.0f, 100.0f) < impatience_)
        {
            SoundHorn();
        }
        //make them more impatient
        impatience_ += impatienceIncSpeed_;
    }

    public void SoundHorn()
    {
        if (!hornSource_.isPlaying)
        {
            hornSource_.clip = hornNoises_[Random.Range(0, hornNoises_.Length)];
            hornSource_.Play();
            impatience_ = baseImpatience_;
        }
    }

    public float GetStopTime()
    {
        return parkingTime_;
    }

    public void SetParkingSpace(ParkingSpace newSpace)
    {
        curParking_ = newSpace;
    }

    public ParkingSpace GetParkingSpace()
    {
        return curParking_;
    }

    public void SetParkingChance(float value)
    {
        parkChance_ = value;
    }

    public float GetImpatience()
    {
        return baseImpatience_;
    }

    public void SetImpatience(float value)
    {
        baseImpatience_ = value;
    }

    public float GetImpatienceIncSpeed()
    {
        return impatienceIncSpeed_;
    }

    public void SetImpatenceIncSpeed(float value)
    {
        impatienceIncSpeed_ = value;
    }

    public float GetParkedAngle()
    {
        return parkedAngle_;
    }

    public void SetParkedAngle(float value)
    {
        parkedAngle_ = value;
    }
}
