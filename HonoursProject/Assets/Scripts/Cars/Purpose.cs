using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Purpose : MonoBehaviour
{
    [SerializeField]
    private float parkChance_;
    [SerializeField]
    private float stoppingTime_;

    private Car car_;

	// Use this for initialization
	void Start ()
    {
        car_ = GetComponent<Car>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void TestPark()
    {
        if(Random.Range(0.0f, 100.0f) < parkChance_)
        {
            Road road = car_.GetCurRoad().GetComponent<Road>();
            ParkingSpace parking = null;
            if(road)
            {
                parking = road.GetParkingSpace().GetComponent<ParkingSpace>();
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
            }
        }   
    }

    public float GetStopTime()
    {
        return stoppingTime_;
    }
}
