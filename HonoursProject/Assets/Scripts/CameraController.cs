﻿using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    private Vector3 targetPos;
	// Use this for initialization
	void Start ()
    {
	    
	}
	
	// Update is called once per frame
	void Update ()
    {
        //see if a car is being controlled
        GameObject player = GameObject.FindGameObjectWithTag("Player");
	    if(player)
        {
            //move the camera behind the player
            transform.position = player.transform.position - (player.transform.forward * 4) + (Vector3.up * 2);
            //look at the player
            transform.LookAt(player.transform.position + (player.transform.forward*3) + (player.GetComponent<Rigidbody>().velocity));
        }
        else if(GameObject.FindGameObjectsWithTag("Car").GetLength(0) > 0)
        {
            //find the average position of the cars
            Vector3 averagePos = Vector3.zero;
            GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
            int numCars = 0;

            foreach (GameObject car in cars)
            {
                averagePos += car.transform.position;
                numCars++;
            }
            averagePos /= numCars;

            targetPos = averagePos;
            targetPos.y = 80.0f;

            transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f * Time.deltaTime);
        }
	}
}
