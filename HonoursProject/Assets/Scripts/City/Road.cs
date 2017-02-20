using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum VertexPosition { VP_TL = 1, VP_TR = 0, VP_BL = 3, VP_BR = 2}

public class Road : MonoBehaviour
{
    public GameObject parkingObject_;
    [SerializeField]
    private Transform start_;
    [SerializeField]
    private Transform end_;
    [SerializeField]
    private Vector2 parkingSize_;

    private Vector3 direction_;

    private GameObject endJunction_;
    private GameObject startJunction_;
    [SerializeField]
    private List<GameObject> parking_;

    //mesh values
    private Vector3[] vertices_;
    private Vector2[] uvs_;
    private int[] triangles_;

    //creates the road mesh after start and end have been set
    public void Init(float width, bool initParking = true)
    {
        //find the forward and right values of the road
        Vector3 forward = (end_.position - start_.position).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
        //set up the start and end points for the vertices
        Vector3 startPos = start_.position - Vector3.up * 0.1f;
        Vector3 endPos = end_.position - Vector3.up * 0.1f;

        //create the new vertices
        vertices_ = new Vector3[4];
        vertices_[0] = endPos + right * width;
        vertices_[1] = endPos - right * width;
        vertices_[2] = startPos + right * width;
        vertices_[3] = startPos - right * width;

        //create the new uv coordinates
        uvs_ = new Vector2[4];
        uvs_[0] = new Vector2(1, 0);
        uvs_[1] = new Vector2(0, 0);
        uvs_[2] = new Vector2(1, 1);
        uvs_[3] = new Vector2(0, 1);

        //set up the triangles
        triangles_ = new int[6];
        triangles_[0] = 0;
        triangles_[1] = 3;
        triangles_[2] = 1;
        triangles_[3] = 0;
        triangles_[4] = 2;
        triangles_[5] = 3;

        //apply the new mesh
        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices_;
        mesh.uv = uvs_;
        mesh.triangles = triangles_;
        mesh.RecalculateNormals();

        //Set up parking 
        parking_ = new List<GameObject>();
        if(initParking)
        {
            float roadLength = (endPos - startPos).magnitude;
            //adjust to trim off ends
            roadLength -= parkingSize_.y;
            int numRoads = Mathf.FloorToInt(roadLength / parkingSize_.y);
            Vector3 parkPos = startPos - right * width - right * parkingSize_.x * 0.5f;
            parkPos += forward * parkingSize_.y;
            for (int i = 0; i < numRoads; i++)
            {
                //create the new parking space
                GameObject newParking = (GameObject)Instantiate(parkingObject_, parkPos, Quaternion.identity);
                //give it the neccessary values
                newParking.transform.SetParent(transform, true);

                newParking.GetComponent<ParkingSpace>().Init(parkingSize_.y, parkingSize_.x, forward);
                //move the position onto the next
                parkPos += forward * parkingSize_.y;

                //add the parking to the list
                parking_.Add(newParking);
            }
        }

        direction_ = forward;
    }

    public void UpdateMesh()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        mesh.Clear();

        mesh.vertices = vertices_;
        mesh.uv = uvs_;
        mesh.triangles = triangles_;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawLine(start_.position, end_.position);
        
    }

    public Junction GetJunction()
    {
        return endJunction_.GetComponent<Junction>();
    }

    public void SetStartJunction(GameObject junction)
    {
        startJunction_ = junction;
    }
    
    public void SetEndJunction(GameObject junction)
    {
        endJunction_ = junction;
    }

    public Transform GetEnd()
    {
        return end_;
    }

    public Transform GetStart()
    {
        return start_;
    }

    public void SetVertex(int index, Vector3 value)
    {
        vertices_[index] = value;
    }

    public Vector3 GetVertex(int index)
    {
        return vertices_[index];
    }

    public GameObject GetParkingSpace()
    {
        foreach(GameObject space in parking_)
        {
            if(space.GetComponent<ParkingSpace>().GetAvailable())
            {
                return space;
            }
        }
        return null;
    }

    public Vector3 GetPointOnRoad(Vector3 target)
    {
        Vector3 roadVec = end_.position - start_.position;
        Vector3 targetVec = target - start_.position;

        float distance = Vector3.Dot(targetVec, roadVec);

        return start_.position + direction_.normalized * distance / roadVec.magnitude;
    }

    public Vector3 GetDirection()
    {
        return direction_;
    }
}
