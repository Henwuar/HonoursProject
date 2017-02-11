using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Car))]
public class Personality : MonoBehaviour
{
    [SerializeField]
    private bool random_;

    [SerializeField]
    private float aggression_;
    [SerializeField]
    private float defensiveness_;
    [SerializeField]
    private float inattentiveness_;

    [SerializeField]
    private Vector2 alterPercentageBounds_;

    private Vision vision_ = null;
    private Error error_ = null;
    private Purpose purpose_ = null;
    private Car car_ = null;

    void Start()
    {
        vision_ = GetComponent<Vision>();
        error_ = GetComponent<Error>();
        purpose_ = GetComponent<Purpose>();
        car_ = GetComponent<Car>();

        if(random_)
        {
            aggression_ = Random.Range(0.0f, 1.0f);
            defensiveness_ = Random.Range(0.0f, 1.0f);
            inattentiveness_ = Random.Range(0.0f, 1.0f);

            //print("a: " + aggression_ + "   d: " + defensiveness_ + "   i: " + inattentiveness_);
        }
    }

    public void Init()
    {
        Start();

        InitAggression();
        InitAttentiveness();
        InitDefensiveness();
    }

    float GetModifier(float value, float modifyingAttribute)
    {
        return value * (Random.Range(alterPercentageBounds_.x, alterPercentageBounds_.y) * modifyingAttribute);
    }


    void InitAggression()
    {
        if (vision_)
        {
            //make them stop shorter
            float stopping = vision_.GetStoppingDistance();
            stopping -= GetModifier(stopping, aggression_);
            vision_.SetStoppingDistance(stopping);

            //make them more focused
            float lookAngle = vision_.GetLookAngle();
            lookAngle -= GetModifier(lookAngle, aggression_);
            vision_.SetLookAngle(lookAngle);
        }

        //increase their top speed
        float topSpeed = car_.GetMaxSpeed();
        topSpeed += GetModifier(topSpeed, aggression_);
        car_.SetMaxSpeed(topSpeed);
    }

    void InitDefensiveness()
    {
        //decrease their top speed
        float topSpeed = car_.GetMaxSpeed();
        topSpeed -= GetModifier(topSpeed, defensiveness_);
        car_.SetMaxSpeed(topSpeed);

        if(vision_)
        {
            //make them look around more
            float lookAngle = vision_.GetLookAngle();
            lookAngle += GetModifier(lookAngle, defensiveness_);
            vision_.SetLookAngle(lookAngle);

            //make them look ahead more
            float visionDistance = vision_.GetVisionDistance();
            visionDistance += GetModifier(visionDistance, defensiveness_);
            vision_.SetVisionDistance(visionDistance);

            //make them stop more cautiously
            float stopping = vision_.GetStoppingDistance();
            stopping += GetModifier(stopping, defensiveness_);
            vision_.SetStoppingDistance(stopping);
        }

        if(error_)
        {
            //reduce the chance of distraction
            float distractionChance = error_.GetDistractionChance();
            distractionChance -= GetModifier(distractionChance, defensiveness_);
            error_.SetDistractionChance(distractionChance);
        }
    }

    void InitAttentiveness()
    {
        if (error_)
        {
            //make them stall more often
            float stallChance = error_.GetStallChance();
            stallChance += GetModifier(stallChance, inattentiveness_);
            error_.SetStallChance(stallChance);

            //increase the chance of distraction
            float distractionChance = error_.GetDistractionChance();
            distractionChance += GetModifier(distractionChance, inattentiveness_);
            error_.SetDistractionChance(distractionChance);
        }
    }
}
