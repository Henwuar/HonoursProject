using UnityEngine;
using System.Collections;

public class LookAtCamera : MonoBehaviour
{
    public bool reverse = true;

	// Update is called once per frame
	void Update ()
    {
        if(reverse)
        {
            Vector3 dir = transform.position - Camera.main.transform.position;
            transform.LookAt(transform.position + dir);
        }
        else
        {
            transform.LookAt(Camera.main.transform);
        }
	}
}
