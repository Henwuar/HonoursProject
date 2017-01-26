using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Car))]
public class Error : MonoBehaviour
{
    [SerializeField]
    private float stallChance_;
    [SerializeField]
    private float stallSpeedMargin_;
    [SerializeField]
    private float ignitionTime_;

    private Car car_;
    private Rigidbody body_;

    private float ignition_ = 0;
    private float stallTime_ = 0;

	// Use this for initialization
	void Start ()
    {
        car_ = GetComponent<Car>();
        body_ = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(car_.GetState() == CarState.CS_STALLED)
        {
            print("stalled");
            if (stallTime_ > 0)
            {
                stallTime_ -= Time.deltaTime;
            }
            else
            {
                car_.SetState(CarState.CS_STARTING);
            }
        }
        if (car_.GetState() == CarState.CS_STARTING)
        {
            print("starting");
            ignition_ += Time.deltaTime;
            if (ignition_ >= ignitionTime_)
            {
                car_.SetState(CarState.CS_MOVING, true);
                ignition_ = 0;
            }
        }
    }

    public void TestStall()
    {
        if(Random.Range(0.0f, 100.0f) < stallChance_ && body_.velocity.magnitude < stallSpeedMargin_)
        {
            print("Stalled");
            car_.SetState(CarState.CS_STALLED);
            stallTime_ = Random.Range(0.0f, ignitionTime_);
            WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>();
            foreach(WheelCollider wheel in wheels)
            {
                wheel.motorTorque = 0.0f;
            }
        }
    }
}
