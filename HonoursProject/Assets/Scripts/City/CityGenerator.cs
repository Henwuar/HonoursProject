﻿using UnityEngine;
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
    private Junction curJunction_;

    private GameObject[] junctions_;
    private GameObject[] cars_;

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
        //store the junctions
        //junctions_ = GameObject.FindGameObjectsWithTag("Junction");

        //destroy all currently existing roads
        foreach(GameObject road in GameObject.FindGameObjectsWithTag("Road"))
        {
            DestroyImmediate(road);
        }

        GameObject.FindGameObjectWithTag("CheckpointManager").GetComponent<CheckpointManager>().Init(maxJunctions, maxJunctions);
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
                //turn off all the colliders for the junctions
                foreach (GameObject junction in junctions_)
                {
                    //junction.GetComponent<Junction>().Finalise();
                    junction.GetComponent<BoxCollider>().enabled = false;
                }

                CreateEntryPoint();
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
        }
    }

    void CreateEntryPoint()
    {
        GameObject firstJunction = junctions_[0];
        transform.position = startPoint_ - Vector3.forward*junctionSpacing_*0.5f;
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
        
        //move the spawn point up slightly
        transform.position = transform.position + Vector3.up;
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
        print(junctions_.Length);
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
