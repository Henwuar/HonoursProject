using UnityEngine;
using System.Collections;

public class CityGenerator : MonoBehaviour
{
    public GameObject junctionPrefab_;
    public GameObject carPrefab_;

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

    private int spawnedCars_ = 0;
    private bool canSpawn_ = false;
    private bool initialised_ = false;
    private float carsInside_ = 0;
    private Junction curJunction_;

    private GameObject[] junctions_;
    private GameObject[] cars_;

	// Use this for initialization
	void Start ()
    {
        //fill a square area with junctions
        GetComponent<BoxCollider>().enabled = false;
        int maxJunctions = Mathf.FloorToInt(size_ / junctionSpacing_);
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
                Instantiate(junctionPrefab_, position, Quaternion.identity, junctionContainer);
            }
        }
        //store the junctions and the cars
        junctions_ = GameObject.FindGameObjectsWithTag("Junction");

        //destroy all currently existing roads
        foreach(GameObject road in GameObject.FindGameObjectsWithTag("Road"))
        {
            DestroyImmediate(road);
        }
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

                canSpawn_ = true;
                initialised_ = true;
                GetComponent<BoxCollider>().enabled = true;
                SpawnCar();
            }
        }
        else if(carsInside_ <= 0)
        {
            canSpawn_ = true;
        }

        if (canSpawn_ && spawnedCars_ < numCars_ && curJunction_.GetLightsOn())
        {
            SpawnCar();
        }
    }

    void SpawnCar()
    {
        int junction = Random.Range(0, junctions_.GetLength(0));
        curJunction_ = junctions_[junction].GetComponent<Junction>();

        transform.position = junctions_[junction].transform.position + Vector3.up;

        GameObject newCar = (GameObject)Instantiate(carPrefab_, transform.position, Quaternion.identity, GameObject.Find("Cars").transform);

        newCar.GetComponent<Car>().Init();
        canSpawn_ = false;
        carsInside_ = 0;
        spawnedCars_++;
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Car")
        {
            //canSpawn_ = true;
            carsInside_--;
        }
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
            carsInside_++;
        }
    }
}
