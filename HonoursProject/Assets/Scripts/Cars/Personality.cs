using UnityEngine;
using System.Collections;

public class Personality : MonoBehaviour
{
    [SerializeField]
    private float aggression_;
    [SerializeField]
    private float defensiveness_;
    [SerializeField]
    private float inattentiveness_;

    [SerializeField]
    private Vector2 alterPercentageBounds_;

    public void Init()
    {
        //aggression values
        Vision vision = GetComponent<Vision>();
        Error error = GetComponent<Error>();
        Purpose purpose = GetComponent<Purpose>();
        if(vision)
        {
            //make them stop shorter
            float stopping = vision.GetStoppingDistance();
            stopping += GetModifier(stopping, aggression_);
            vision.SetStoppingDistance(stopping);

            //make them more focused
            float lookAngle = vision.GetLookAngle();
            lookAngle += GetModifier(lookAngle, aggression_);
            vision.SetLookAngle(lookAngle);
        }
    }

    float GetModifier(float value, float modifyingAttribute)
    {
        return value * (Random.Range(alterPercentageBounds_.x, alterPercentageBounds_.y) * modifyingAttribute);
    }

}
