using UnityEngine;
using System.Collections;

public class CityGenerator : MonoBehaviour
{
    public GameObject junctionPrefab_;
    public GameObject carPrefab_;
    public GameObject buildingPrefab_;

    [SerializeField]
    private float size_;
    [SerializeField]
    private Vector3 startPoint_;
    [SerializeField]
    private float junctionSpacing_;
    [SerializeField]
    private int numCars_;
    [SerializeField]
    private float randomisation_;
    [SerializeField]
    private Vector2 buildingHeightBounds_;

    private int spawnedCars_ = 0;
    private bool canSpawn_ = false;
    private bool initialised_ = false;
    private int curEntryPoint_ = 0;

    private GameObject[] junctions_;
    private GameObject[] cars_;
    private Vector3[] entryPoints_;

	// Use this for initialization
	void Start ()
    {
        //fill a square area with junctions
        GetComponent<BoxCollider>().enabled = false;
        int maxJunctions = Mathf.FloorToInt(size_ / junctionSpacing_);
        junctions_ = new GameObject[maxJunctions * maxJunctions];
        Transform junctionContainer = GameObject.Find("Junctions").transform;
        for(int x = 0; x < maxJunctions; x++)
        {
            for(int y = 0; y < maxJunctions; y++)
            {
                //get a random value to move the junction by
                Vector3 random = Random.insideUnitCircle * randomisation_;
                random.y = 0;
                //set up the position of the junction
                Vector3 position = startPoint_ + new Vector3(x * junctionSpacing_, 0, y * junctionSpacing_) + random;
                //create the junction at the position inside the junction container
                GameObject newJunction = (GameObject)Instantiate(junctionPrefab_, position, Quaternion.identity, junctionContainer);

                junctions_[maxJunctions * x + y] = newJunction;
            }
        }

        //destroy all currently existing roads
        foreach(GameObject road in GameObject.FindGameObjectsWithTag("Road"))
        {
            DestroyImmediate(road);
        }

        //set up the ground plane
        GameObject ground = GameObject.FindGameObjectWithTag("Ground");
        Vector3 endPoint = startPoint_ + new Vector3(size_, 0, size_);
        ground.transform.position = Vector3.Lerp(startPoint_, endPoint, 0.5f) - Vector3.up * 0.11f;
        float scale = size_ * 0.125f;
        ground.transform.localScale = new Vector3(scale, scale, scale);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(!initialised_)
        {
            bool canStart = true;
            foreach (GameObject junction in junctions_)
            {
                if (!junction.GetComponent<Junction>().Ready())
                {
                    canStart = false;
                    break;
                }
            }
            if (canStart)
            {
                //initialise the checkpoints
                int maxJunctions = Mathf.FloorToInt(size_ / junctionSpacing_);
                GameObject.FindGameObjectWithTag("CheckpointManager").GetComponent<CheckpointManager>().Init(maxJunctions, maxJunctions);

                //turn off all the colliders for the junctions
                foreach (GameObject junction in junctions_)
                {
                    junction.GetComponent<Junction>().InitLightRunner();
                    junction.GetComponent<BoxCollider>().enabled = false;
                }

                entryPoints_ = new Vector3[maxJunctions*4];
                for(int index = 0; index < maxJunctions*4; index++)
                {
                    int side = Mathf.FloorToInt(index / maxJunctions);

                    switch(side)
                    {
                        case 0:
                            entryPoints_[index] = CreateEntryPoint(index, -Vector3.right);
                            break;
                        case 1:
                            entryPoints_[index] = CreateEntryPoint(junctions_.Length - (index % maxJunctions) - 1, Vector3.right);
                            break;
                        case 2:
                            entryPoints_[index] = CreateEntryPoint((index % maxJunctions) * maxJunctions, -Vector3.forward);
                            break;
                        case 3:
                            entryPoints_[index] = CreateEntryPoint((index % maxJunctions) * maxJunctions + maxJunctions - 1, Vector3.forward);
                            break;
                    }
                }

                transform.position = entryPoints_[0] + Vector3.up;

                CreateBuildings();

                canSpawn_ = true;
                initialised_ = true;
                GetComponent<BoxCollider>().enabled = true;
                SpawnCar();
            }
        }

        if (canSpawn_ && spawnedCars_ < numCars_)
        {
            SpawnCar();
            if(spawnedCars_ >= numCars_)
            {
                print("done");
            }
        }


        /*if (!Physics.CheckBox(transform.position, GetComponent<BoxCollider>().size * 0.5f, Quaternion.identity, LayerMask.NameToLayer("CarCheck")))
        {
            canSpawn_ = true;
        }*/
        /*if(spawnedCars_ < numCars_)
        {
            Time.timeScale = 5.0f;
            if (!Physics.CheckBox(transform.position, GetComponent<BoxCollider>().size * 0.5f, Quaternion.identity, LayerMask.NameToLayer("CarCheck")))
            {
                canSpawn_ = true;
            }
        }
        else
        {
            Time.timeScale = 1.0f;
        }*/

        if (initialised_)
        {
            //move the generator along the entry points
            curEntryPoint_++;
            if (curEntryPoint_ >= entryPoints_.Length)
            {
                curEntryPoint_ = 0;
            }
            transform.position = entryPoints_[curEntryPoint_] + Vector3.up;
        }
    }

    Vector3 CreateEntryPoint(int index, Vector3 direction)
    {
        GameObject firstJunction = junctions_[index];
        transform.position = firstJunction.transform.position + direction*junctionSpacing_*0.5f;
        GameObject road = firstJunction.GetComponent<Junction>().road_;
        float laneSpacing = firstJunction.GetComponent<Junction>().GetLaneSpacing();
        //create a new road
        GameObject newRoad = (GameObject)Instantiate(road, GameObject.Find("Roads").transform);
        //get the offset values
        Vector3 laneOffset = Vector3.Cross(firstJunction.transform.position - transform.position, Vector3.up).normalized * laneSpacing;
        Vector3 sizeOffset = (firstJunction.transform.position - transform.position).normalized * firstJunction.GetComponent<Junction>().GetSize();
        //set up the values of the road
        newRoad.GetComponent<Road>().GetStart().position = transform.position + sizeOffset + laneOffset;
        newRoad.GetComponent<Road>().GetEnd().position = firstJunction.transform.position - sizeOffset + laneOffset;
        newRoad.GetComponent<Road>().SetEndJunction(firstJunction);
        newRoad.GetComponent<Road>().SetStartJunction(gameObject);
        newRoad.GetComponent<Road>().GetEnd().transform.FindChild("TrafficLight").transform.LookAt(newRoad.GetComponent<Road>().GetStart().position);
        firstJunction.GetComponent<Junction>().AddLight(newRoad.GetComponent<Road>().GetEnd().FindChild("TrafficLight").gameObject);
        newRoad.GetComponent<Road>().Init(laneSpacing, false);

        return transform.position;

        //move the spawn point up slightly
        //transform.position = transform.position + Vector3.up;
    }

    void CreateBuildings()
    {
        int citySideSize = Mathf.FloorToInt(size_ / junctionSpacing_);
        GameObject buildingContainer = GameObject.FindGameObjectWithTag("Buildings");

        for (int x = 0; x < citySideSize-1; x++)
        {
            for(int y = 0; y < citySideSize-1; y++)
            {
                int index = (citySideSize * x) + y;
                
                float xMin, xMax, zMin, zMax;
                Transform[] curJunctions = { junctions_[index].transform, junctions_[index + 1].transform, junctions_[index + citySideSize].transform, junctions_[index + citySideSize + 1].transform };
                //set up the bounds
                xMin = Mathf.Min(curJunctions[0].position.x, curJunctions[1].position.x, curJunctions[2].position.x, curJunctions[3].position.x);
                xMax = Mathf.Max(curJunctions[0].position.x, curJunctions[1].position.x, curJunctions[2].position.x, curJunctions[3].position.x);
                zMin = Mathf.Min(curJunctions[0].position.z, curJunctions[1].position.z, curJunctions[2].position.z, curJunctions[3].position.z);
                zMax = Mathf.Max(curJunctions[0].position.z, curJunctions[1].position.z, curJunctions[2].position.z, curJunctions[3].position.z);

                Vector3 newPos = new Vector3(Mathf.Lerp(xMin, xMax, 0.5f), 0.0f, Mathf.Lerp(zMin, zMax, 0.5f));
                Vector3 newScale = new Vector3(xMax - xMin, 0, zMax - zMin) * 0.6f;
                newScale.y = Random.Range(buildingHeightBounds_.x, buildingHeightBounds_.y);

                GameObject newBuilding = (GameObject)Instantiate(buildingPrefab_, newPos, Quaternion.identity, buildingContainer.transform);
                newBuilding.transform.localScale = newScale;
            }
            
        }
    }

    void SpawnCar()
    {
        //int junction = Random.Range(0, junctions_.GetLength(0));
        //curJunction_ = junctions_[junction].GetComponent<Junction>();

        //transform.position = junctions_[junction].transform.position + Vector3.up;

        GameObject newCar = (GameObject)Instantiate(carPrefab_, transform.position, Quaternion.identity, GameObject.Find("Cars").transform);

        newCar.GetComponent<Car>().Init();
        canSpawn_ = false;
        spawnedCars_++;
    }

    void OnTriggerExit(Collider other)
    {
        canSpawn_ = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Car")
        {
            canSpawn_ = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Car")
        {
            //carsInside_++;
            canSpawn_ = false;
        }
    }
}
