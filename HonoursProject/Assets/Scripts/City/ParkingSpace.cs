using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParkingSpace : MonoBehaviour
{
    [SerializeField]
    private bool available_;
    private float length_;

    public void SetAvailable(bool value)
    {
        available_ = value;
    }

    public bool GetAvailable()
    {
        return available_;
    }

    public void SetLength(float value)
    {
        length_ = value;
    }

    public List<Vector3> GetEnterTargets()
    {
        List<Vector3> targets = new List<Vector3>();

        Road parentRoad = GetComponentInParent<Road>();
        if(parentRoad)
        {
            //add the first target - point on road before parking
            targets.Add(parentRoad.GetPointOnRoad(transform.position - transform.forward*length_));
            //move the car into the spot
            targets.Add(transform.position - transform.forward * length_*0.25f);
            //add the final stopping place
            targets.Add(transform.position);
        }

        return targets;
    }

    public List<Vector3> GetExitTargets()
    {
        List<Vector3> targets = new List<Vector3>();

        Road parentRoad = GetComponentInParent<Road>();
        if (parentRoad)
        {
            //add the first target - make car reverse slightly
            targets.Add(transform.position - transform.forward * length_ * 0.25f);
            //move the car onto the road
            targets.Add(parentRoad.GetPointOnRoad(transform.position + transform.forward * length_));
        }

        return targets;
    }

}
