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
            targets.Add(parentRoad.GetPointOnRoad(transform.position - transform.forward*length_*0.5f));
            //add this position
            targets.Add(transform.position);
        }

        return targets;
    }

}
