using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum Directions { D_NORTH = 0, D_SOUTH = 1, D_WEST = 2, D_EAST = 3};

public class Junction : MonoBehaviour
{
    public GameObject road_;

    [SerializeField]
    private List<GameObject> roads_;
    [SerializeField]
    private bool oneWay_;
    [SerializeField]
    private float size_;
    [SerializeField]
    private float laneSpacing_;
    [SerializeField]
    private float lightTiming_;

    private Vector3 prevPosition_;
    private Stack<Vector3> checkDirections_;
    private bool ready_;

    private List<GameObject> lights_;
    private int lightRunner_;
    private float timer_;

    void Start()
    {
        ready_ = false;
        prevPosition_ = transform.position;
        checkDirections_ = new Stack<Vector3>();
        lights_ = new List<GameObject>();
        FindConnections();
        timer_ = 0;
        lightRunner_ = 0;
    }

	// Use this for initialization
	void OnAwake ()
    {
        Start();
    }
	
	// Update is called once per frame
	void Update ()
    {
        //find the connected roads
        if(checkDirections_.Count > 0)
        {
            //raycast out in the first direction to look for a connection
            RaycastHit hit;
            Physics.Raycast(transform.position, checkDirections_.Pop(), out hit);
            //a junction was found
            if (hit.collider && hit.collider.gameObject.tag == "Junction")
            {
                //get the hit object
                GameObject otherJunction = hit.collider.gameObject;
                //create a new road
                GameObject newRoad = (GameObject)Instantiate(road_, GameObject.Find("Roads").transform);
                //get the offset values
                Vector3 laneOffset = Vector3.Cross(otherJunction.transform.position - transform.position, Vector3.up).normalized * laneSpacing_;
                Vector3 sizeOffset = (otherJunction.transform.position - transform.position).normalized * size_;
                //set up the values of the road
                newRoad.GetComponent<Road>().start_.position = transform.position + sizeOffset + laneOffset;
                newRoad.GetComponent<Road>().end_.position = otherJunction.transform.position - sizeOffset + laneOffset;
                newRoad.GetComponent<Road>().endJunction_ = otherJunction;
                newRoad.GetComponent<Road>().end_.transform.FindChild("TrafficLight").transform.LookAt(newRoad.GetComponent<Road>().start_.position);
                otherJunction.GetComponent<Junction>().AddLight(newRoad.GetComponent<Road>().end_.transform.FindChild("TrafficLight").gameObject);
                //add the road to the list
                roads_.Add(newRoad);
            }
        }
        else if(!ready_)
        {
            ready_ = true;
        }

        timer_ += Time.deltaTime;
        if(timer_ > lightTiming_)
        {
            timer_ = 0;
            lightRunner_++;
            if(lightRunner_ >= lights_.Count)
            {
                lightRunner_ = 0;
            }
            //update the lights
            for(int i = 0; i < lights_.Count; i++)
            {
                if(i == lightRunner_)
                {
                    lights_[i].GetComponent<TrafficLight>().SetSignal(Signals.S_GO);
                }
                else
                {
                    lights_[i].GetComponent<TrafficLight>().SetSignal(Signals.S_STOP);
                }
            }
        }
    }

    public bool Ready()
    {
        return ready_;
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
            //targets.Add(roads_[id].GetComponent<Road>().endJunction_.transform);
        }
        return targets;
    }

    public void AddLight(GameObject light)
    {
        lights_.Add(light);
    }

    void FindConnections()
    {
        //clear the existing roads
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
