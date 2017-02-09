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
    [SerializeField]
    private float distractionChance_;
    [SerializeField]
    private float distractionTime_;
    [SerializeField]
    private int distractionCheckFrames_;

    private Car car_;
    private Vision vision_;
    private Rigidbody body_;
    private EventSender eventSender_;

    private float ignition_ = 0;
    private float stallTime_ = 0;
    private bool stalled_ = false;
    private bool distracted_ = false;
    private float distractedTime_ = 0;
    private int frameCounter_ = 0;

	// Use this for initialization
	void Start ()
    {
        car_ = GetComponent<Car>();
        body_ = GetComponent<Rigidbody>();
        vision_ = GetComponent<Vision>();
        eventSender_ = GetComponent<EventSender>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        //wait to realise what's happened
        if(stalled_)
        {
            if (car_.GetState() == CarState.CS_STALLED)
            {
                if (stallTime_ > 0)
                {
                    stallTime_ -= Time.deltaTime;
                }
                else
                {
                    car_.SetState(CarState.CS_STARTING);
                }
            }
            //try and start up the car again
            if (car_.GetState() == CarState.CS_STARTING)
            {
                ignition_ += Time.deltaTime;
                if (ignition_ >= ignitionTime_)
                {
                    car_.SetState(CarState.CS_MOVING, true);
                    ignition_ = 0;
                    stalled_ = false;
                }
            }
        }
        if(vision_.enabled)
        {
            if (!distracted_)
            {
                frameCounter_++;
                if(frameCounter_ >= distractionCheckFrames_)
                {
                    frameCounter_ = 0;
                    if (Random.Range(0.0f, 100.0f) < distractionChance_)
                    {
                        distracted_ = true;
                        distractedTime_ = distractionTime_;
                        float maxAngle = vision_.GetLookAngle() * 2.0f;
                        vision_.SetLookStartAngle(Random.Range(-maxAngle, maxAngle));
                        eventSender_.SendEvent(TrafficEvent.TE_DISTRACTED);
                    }
                }
            }
            else
            {
                distractedTime_ -= Time.deltaTime;
                if(distractedTime_ <= 0)
                {
                    distracted_ = false;
                    vision_.SetLookStartAngle(0);
                }
            }
        }
        
    }

    public void TestStall()
    {
        if(Random.Range(0.0f, 100.0f) < stallChance_ && body_.velocity.magnitude <= stallSpeedMargin_)
        {
            car_.SetState(CarState.CS_STALLED);
            stallTime_ = Random.Range(0.0f, ignitionTime_);
            stalled_ = true;

            WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>();
            foreach(WheelCollider wheel in wheels)
            {
                wheel.motorTorque = 0.0f;
            }
            eventSender_.SendEvent(TrafficEvent.TE_STALLED);
        }
    }

    public void SetDistracted(bool value)
    {
        distracted_ = false;
    }

    public bool GetDistracted()
    {
        return distracted_;
    }
}
