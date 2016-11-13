using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum Directions { D_NORTH = 0, D_SOUTH = 1, D_WEST = 2, D_EAST = 3};

[ExecuteInEditMode]
public class Junction : MonoBehaviour
{
    public GameObject road_;

    [SerializeField]
    private List<GameObject> roads_;
    [SerializeField]
    private bool oneWay_;
    


    private Vector3 prevPosition_;
    private Stack<Vector3> checkDirections_;

    void Start()
    {
        OnAwake();
    }

	// Use this for initialization
	void OnAwake ()
    {
        prevPosition_ = transform.position;
        checkDirections_ = new Stack<Vector3>();
        FindConnections();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(prevPosition_ != transform.position)
        {
            FindConnections();
        }
        if(checkDirections_.Count > 0)
        {
            RaycastHit hit;
            //forward
            Physics.Raycast(transform.position, checkDirections_.Pop(), out hit);
            if (hit.collider && hit.collider.gameObject.tag == "Junction")
            {
                GameObject otherJunction = hit.collider.gameObject;
                GameObject newRoad = Instantiate(road_);
                Vector3 laneOffset = Vector3.Cross(otherJunction.transform.position - transform.position, Vector3.up).normalized;
                Vector3 offset = (otherJunction.transform.position - transform.position).normalized;
                newRoad.GetComponent<Road>().start_.position = transform.position + offset + laneOffset;
                newRoad.GetComponent<Road>().end_.position = otherJunction.transform.position - offset + laneOffset;
                newRoad.GetComponent<Road>().endJunction_ = otherJunction;
                roads_.Add(newRoad);
            }
        }

        Debug.DrawLine(transform.position + Vector3.up, transform.position - Vector3.up, Color.red);
        Debug.DrawLine(transform.position + Vector3.forward, transform.position - Vector3.forward, Color.red);
        Debug.DrawLine(transform.position + Vector3.right, transform.position - Vector3.right, Color.red);

        prevPosition_ = transform.position;
    }

    public IEnumerable<Transform> GetNewTargets()
    {
        List<Transform> targets = new List<Transform>();
        //make sure there are connected junctions
        if(roads_.Count == 0)
        {
            targets.Add(gameObject.transform);
        }
        else
        {
            int id = Random.Range(0, roads_.Count);
            targets.Add(roads_[id].GetComponent<Road>().start_);
            targets.Add(roads_[id].GetComponent<Road>().end_);
            targets.Add(roads_[id].GetComponent<Road>().endJunction_.transform);
        }
        return targets;
    }

    void FindConnections()
    {
        foreach(GameObject road in roads_)
        {
            DestroyImmediate(road);
        }
        roads_ = new List<GameObject>();
        //find any junctions in the 4 polar directions
        checkDirections_.Clear();

        checkDirections_.Push(Vector3.forward);
        checkDirections_.Push(-Vector3.forward);
        checkDirections_.Push(Vector3.right);
        checkDirections_.Push(-Vector3.right);
    }
}
