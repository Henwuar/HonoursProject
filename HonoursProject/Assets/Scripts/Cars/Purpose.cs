using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Purpose : MonoBehaviour
{
    [SerializeField]
    private float parkChance_;
    [SerializeField]
    private float parkingTime_;

    private Car car_;
    private ParkingSpace curParking_ = null;

    private float parkTimer_;

	// Use this for initialization
	void Start ()
    {
        car_ = GetComponent<Car>();
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

    public float GetStopTime()
    {
        return parkingTime_;
    }
}
