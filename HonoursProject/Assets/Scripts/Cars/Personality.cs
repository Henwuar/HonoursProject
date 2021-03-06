﻿using UnityEngine;
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

    private bool initialised = false;

    private float baseStopping;
    private float baseLookAngle;
    private float baseImpatience;
    private float baseImpatienceSpeed;
    private float baseTopSpeed;
    private float baseVisionDistance;
    private float baseDistractionChance;
    private float baseStallChance;
    private float baseParkChance;
    private float baseReactionTime;


    void OnDisable()
    {
        if(initialised)
        {
            ResetToBase();
        }
    }

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

            if(purpose_)
            {
                purpose_.SetParkingChance(Random.Range(5, 50));
            }
        }
    }

    public void Init(bool overrideValues = false, float aggression = 0, float defensiveness = 0, float inattentiveness = 0)
    {
        Start();

        if (overrideValues)
        {
            aggression_ = aggression;
            defensiveness_ = defensiveness;
            inattentiveness_ = inattentiveness;
            ResetToBase();
        }
        else
        {
            StoreBaseValues();
        }

        InitAggression();
        InitAttentiveness();
        InitDefensiveness();

        initialised = true;
    }

    public void ResetToBase()
    {
        InitAggression();
        InitAttentiveness();
        InitDefensiveness();
    }

    float GetModifier(float value, float modifyingAttribute)
    {
        return value * (Random.Range(alterPercentageBounds_.x, alterPercentageBounds_.y) * modifyingAttribute);
    }


    void StoreBaseValues()
    { 
        baseTopSpeed = car_.GetMaxSpeed();

        if (vision_)
        {
            baseStopping = vision_.GetStoppingDistance();
            baseLookAngle = vision_.GetLookAngle();
            baseVisionDistance = vision_.GetVisionDistance();
        }
        if(purpose_)
        {
            baseImpatience = purpose_.GetImpatience();
            baseImpatienceSpeed = purpose_.GetImpatienceIncSpeed();
            baseParkChance = purpose_.GetParkChance();
        }
        if(error_)
        {
            baseDistractionChance = error_.GetDistractionChance();
            baseStallChance = error_.GetStallChance();
            baseReactionTime = error_.GetReactionTime();
        }
    }

    void InitAggression()
    {
        if (vision_)
        {
            //make them stop shorter
            float stopping = baseStopping;
            stopping -= GetModifier(stopping, aggression_);
            vision_.SetStoppingDistance(stopping);

            //make them pay attention to only the road ahead
            float lookAngle = baseLookAngle;
            lookAngle -= GetModifier(lookAngle, aggression_);
            vision_.SetLookAngle(lookAngle);
        }

        if(purpose_)
        {
            float impatience = baseImpatience;
            impatience += GetModifier(impatience, aggression_);
            purpose_.SetImpatience(impatience);

            float impatienceSpeed = baseImpatienceSpeed;
            impatienceSpeed += GetModifier(impatienceSpeed, aggression_);
            purpose_.SetImpatenceIncSpeed(impatienceSpeed);
        }

        //increase their top speed
        float topSpeed = baseTopSpeed;
        topSpeed += GetModifier(topSpeed, aggression_);
        car_.SetMaxSpeed(topSpeed);
    }

    void InitDefensiveness()
    {
        //decrease their top speed
        float topSpeed = baseTopSpeed;
        topSpeed -= GetModifier(topSpeed, defensiveness_);
        car_.SetMaxSpeed(topSpeed);

        if(vision_)
        {
            //make them look around more
            float lookAngle = baseLookAngle;
            lookAngle += GetModifier(lookAngle, defensiveness_);
            vision_.SetLookAngle(lookAngle);

            //make them look ahead more
            float visionDistance = baseVisionDistance;
            visionDistance += GetModifier(visionDistance, defensiveness_);
            vision_.SetVisionDistance(visionDistance);

            //make them stop more cautiously
            float stopping = baseStopping;
            stopping += GetModifier(stopping, defensiveness_);
            vision_.SetStoppingDistance(stopping);
        }

        if(error_)
        {
            //reduce the chance of distraction
            float distractionChance = baseDistractionChance;
            distractionChance -= GetModifier(distractionChance, defensiveness_);
            error_.SetDistractionChance(distractionChance);
        }

        if (purpose_)
        {
            //increase the chance of parking
            float parkChance = baseParkChance;
            parkChance += GetModifier(parkChance, inattentiveness_);
            purpose_.SetParkingChance(parkChance);
        }
    }

    void InitAttentiveness()
    {
        if (error_)
        {
            //make them stall more often
            float stallChance = baseStallChance;
            stallChance += GetModifier(stallChance, inattentiveness_);
            error_.SetStallChance(stallChance);

            //increase the chance of distraction
            float distractionChance = baseDistractionChance;
            distractionChance += GetModifier(distractionChance, inattentiveness_);
            error_.SetDistractionChance(distractionChance);

            //increase the reaction time
            float reactionTime = baseReactionTime;
            reactionTime += GetModifier(reactionTime, inattentiveness_);
            error_.SetReactionTime(reactionTime);
        }

        if(purpose_)
        {
            float parkChance = baseParkChance;
            parkChance -= GetModifier(parkChance, inattentiveness_);
            purpose_.SetParkingChance(parkChance);
        }
    }

    public float GetAggression()
    {
        return aggression_;
    }

    public float GetInattentiveness()
    {
        return inattentiveness_;
    }

    public float GetDefensiveness()
    {
        return defensiveness_;
    }
}
