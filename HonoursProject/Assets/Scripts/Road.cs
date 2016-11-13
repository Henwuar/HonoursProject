using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Road : MonoBehaviour
{
    public Transform start_;
    public Transform end_;
    public GameObject endJunction_;

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(start_.position, end_.position);
    }
}
