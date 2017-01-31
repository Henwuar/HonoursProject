using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum Directions { D_NORTH = 0, D_EAST = 1, D_SOUTH = 2, D_WEST = 3};

public class Junction : MonoBehaviour
{
    public GameObject road_;

    [SerializeField]
    private List<GameObject> roads_;
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
    [SerializeField]
    private GameObject[] dirRoads_;
    private bool lightsOn_ = true;

    void Start()
    {
        ready_ = false;
        prevPosition_ = transform.position;
        checkDirections_ = new Stack<Vector3>();
        lights_ = new List<GameObject>();
        FindConnections();
        timer_ = 0;
        lightRunner_ = 0;
        dirRoads_ = new GameObject[] { null, null, null, null, null, null, null, null };
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
            Vector3 checkDir = checkDirections_.Pop();
            Physics.Raycast(transform.position, checkDir, out hit);
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
                newRoad.GetComponent<Road>().GetStart().position = transform.position + sizeOffset + laneOffset;
                newRoad.GetComponent<Road>().GetEnd().position = otherJunction.transform.position - sizeOffset + laneOffset;
                newRoad.GetComponent<Road>().SetEndJunction(otherJunction);
                newRoad.GetComponent<Road>().SetStartJunction(gameObject);
                newRoad.GetComponent<Road>().GetEnd().transform.FindChild("TrafficLight").transform.LookAt(newRoad.GetComponent<Road>().GetStart().position);
                otherJunction.GetComponent<Junction>().AddLight(newRoad.GetComponent<Road>().GetEnd().FindChild("TrafficLight").gameObject);
                newRoad.GetComponent<Road>().Init(laneSpacing_);
                //add the road to the list
                roads_.Add(newRoad);
                dirRoads_[(int)GetDirection(checkDir)] = newRoad;
            }
        }
        else if(!ready_)
        {
            ready_ = true;
        }

        //update the traffic lights
        timer_ += Time.deltaTime;
        if(timer_ > lightTiming_)
        {
            timer_ = 0;
            lightsOn_ = !lightsOn_;
            if(lightsOn_)
            {
                lightRunner_++;
                if (lightRunner_ >= lights_.Count)
                {
                    lightRunner_ = 0;
                }
            }
            
            //update the lights
            for(int i = 0; i < lights_.Count; i++)
            {
                if(i == lightRunner_ && lightsOn_)
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

    /*public List<Vector3> GetNewTargets()
    {
        List<Vector3> targets = new List<Vector3>();
        //make sure there are connected junctions
        if(roads_.Count == 0)
        {
            targets.Add(gameObject.transform.position);
        }
        else
        {
            int id = Random.Range(0, roads_.Count);
            targets.Add(roads_[id].GetComponent<Road>().GetStart().position);
            targets.Add(roads_[id].GetComponent<Road>().GetEnd().position);
            //targets.Add(roads_[id].GetComponent<Road>().endJunction_.transform);
        }
        return targets;
    }*/

    public GameObject GetNewRoad(GameObject curRoad)
    {
        GameObject newRoad = null;
        if(roads_.Count > 0)
        {
            int id = Random.Range(0, roads_.Count);
            newRoad = roads_[id];
            //make sure the newroad isnt the opposite direction of the current road
            while(Vector3.Angle(newRoad.GetComponent<Road>().GetDirection(), curRoad.GetComponent<Road>().GetDirection()) == 180)
            {
                id = Random.Range(0, roads_.Count);
                newRoad = roads_[id];
            }
        }
        return newRoad;
    }

    public void AddLight(GameObject light)
    {
        lights_.Add(light);
    }

    public void Finalise()
    {
        //add the roads that end at this junction
        foreach(GameObject trafficLight in lights_)
        {
            dirRoads_[GetDirection(-trafficLight.transform.forward) + 4] = trafficLight.transform.parent.transform.parent.gameObject;
        }

        //set up the mesh of the junction
        Vector3[] vertices = new Vector3[5];
        Vector2[] uvs = new Vector2[5];
        int[] triangles;
        //centre vertex
        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);
        //set up corner vertices
        for(int index = 0; index < 4; index++)
        {
            //get the two junction entrances that will be used
            int otherDirection = index == 0 ? 3 : (index - 1);
            GameObject curRoad = dirRoads_[index];
            GameObject otherRoad = dirRoads_[otherDirection+4];
            //find the starting point
            Vector3 point = transform.position + (Quaternion.Euler(0, -45 + (90 * index), 0) * Vector3.forward * size_);
            if (curRoad != null)
            {
                point = curRoad.GetComponent<Road>().GetVertex((int)VertexPosition.VP_BL);
                if (otherRoad != null)
                {
                    //get the average of the two vertices
                    point += otherRoad.GetComponent<Road>().GetVertex((int)VertexPosition.VP_TL);
                    point *= 0.5f;
                    //update the vertices of the roads
                    curRoad.GetComponent<Road>().SetVertex((int)VertexPosition.VP_BL, point);
                    curRoad.GetComponent<Road>().UpdateMesh();
                    otherRoad.GetComponent<Road>().SetVertex((int)VertexPosition.VP_TL, point);
                    otherRoad.GetComponent<Road>().UpdateMesh();
                }
            }
            else if (otherRoad != null)
            {
                point = otherRoad.GetComponent<Road>().GetVertex((int)VertexPosition.VP_TL);
            }
            vertices[index + 1] = point - transform.position;
            vertices[index + 1].y = 0.08f;
            uvs[index + 1] = new Vector2(0, 0);
        }

        triangles = new int[]
        {
            1, 3, 4,
            1, 2, 3
        };

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

    }

    public bool GetLightsOn()
    {
        return lightsOn_;
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
        checkDirections_.Push(-Vector3.right);
        checkDirections_.Push(Vector3.right);
    }

    int GetDirection(Vector3 direction)
    {
        float angle = Vector3.Angle(Vector3.forward, direction);
        if(angle > 225 && angle <= 315)
        {
            return 3;
        }
        else if (angle > 45 && angle <= 135) 
        {
            return 1;
        }
        else if (angle > 135 && angle <= 225)
        {
            return 2;
        }
        else
        {
            return 0;
        }
    }
}
