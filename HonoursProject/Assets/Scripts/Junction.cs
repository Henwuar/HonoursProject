using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class Junction : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> connectedJunctions_;
    [SerializeField]
    private bool oneWay_;

	// Use this for initialization
	void Start ()
    {
        if(!oneWay_)
        {
            foreach(GameObject junction in connectedJunctions_)
            {
                junction.GetComponent<Junction>().AddJunction(gameObject);
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        Debug.DrawLine(transform.position + Vector3.up, transform.position - Vector3.up, Color.red);
        Debug.DrawLine(transform.position + Vector3.forward, transform.position - Vector3.forward, Color.red);
        Debug.DrawLine(transform.position + Vector3.right, transform.position - Vector3.right, Color.red);

        foreach(GameObject junction in connectedJunctions_)
        {
            Debug.DrawLine(transform.position, junction.transform.position, Color.blue);
        }
    }

    public GameObject GetJunction()
    {
        int id = Random.Range(0, connectedJunctions_.Count);
        return connectedJunctions_[id];
    }

    public void AddJunction(GameObject junction)
    {
        connectedJunctions_.Add(junction);
    }
}
