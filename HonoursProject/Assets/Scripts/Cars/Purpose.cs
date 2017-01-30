using UnityEngine;
using System.Collections;

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
            car_.SetState(CarState.CS_PARKING);
            GameObject newTarget = car_.GetCurTarget().parent.gameObject.GetComponent<Road>().GetParkingSpace();

            //print(newTarget);
            if (newTarget)
            {
                newTarget.GetComponent<ParkingSpace>().available = false;
                car_.SetTarget(newTarget.transform);
            }
        }   
    }

    public float GetStopTime()
    {
        return stoppingTime_;
    }
}
