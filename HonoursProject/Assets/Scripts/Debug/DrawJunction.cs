using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DrawJunction : MonoBehaviour
{
	
	// Update is called once per frame
	void Update ()
    {
        Debug.DrawLine(transform.position + Vector3.up, transform.position - Vector3.up, Color.red);
        Debug.DrawLine(transform.position + Vector3.forward, transform.position - Vector3.forward, Color.red);
        Debug.DrawLine(transform.position + Vector3.right, transform.position - Vector3.right, Color.red);
    }
}
