using UnityEngine;
using System.Collections;

public class CityGenerator : MonoBehaviour
{
    private GameObject[] junctions_;
    private GameObject[] cars_;

	// Use this for initialization
	void Start ()
    {
        //store the junctions and the cars
        junctions_ = GameObject.FindGameObjectsWithTag("Junction");
        cars_ = GameObject.FindGameObjectsWithTag("Car");
        foreach(GameObject car in cars_)
        {
            car.SetActive(false);
        }
        //destroy all currently existing roads
        foreach(GameObject road in GameObject.FindGameObjectsWithTag("Road"))
        {
            DestroyImmediate(road);
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
        bool canStart = true;
        foreach(GameObject junction in junctions_)
        {
            if(!junction.GetComponent<Junction>().Ready())
            {
                canStart = false;
                break;
            }
        }
        if(canStart)
        {
            print("ready");
            //activate all the cars
            foreach(GameObject car in cars_)
            {
                car.SetActive(true);
                car.GetComponent<Car>().Init();
            }
            //turn off all the colliders for the junctions
            foreach(GameObject junction in junctions_)
            {
                junction.GetComponent<BoxCollider>().enabled = false;
            }
            //turn off this object
            gameObject.SetActive(false);
        }
	}
}
