using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParkingSpace : MonoBehaviour
{
    [SerializeField]
    private bool available_;
    private float length_;

    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.red);
    }

    public void SetAvailable(bool value)
    {
        available_ = value;
    }

    public bool GetAvailable()
    {
        return available_;
    }

    public void SetLength(float value)
    {
        length_ = value;
    }

    public void Init(float length, float width, Vector3 forward)
    {
        Vector3[] vertices = new Vector3[6];
        Vector2[] uvs = new Vector2[6];
        int[] triangles;

        //tl
        vertices[0] = transform.forward * length * 0.5f - transform.right*width*0.5f;
        //tr
        vertices[1] = transform.forward * length * 0.5f + transform.right * width * 0.5f;
        //ml
        vertices[2] = -transform.right * width * 0.5f;
        //mr
        vertices[3] = transform.right * width * 0.5f;
        //bl
        vertices[4] = -transform.forward * length * 0.5f - transform.right * width * 0.5f;
        //br
        vertices[5] = -transform.forward * length * 0.5f + transform.right * width * 0.5f;
        
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(1, 1);
        uvs[4] = new Vector2(0, 0);
        uvs[5] = new Vector2(1, 0);

        triangles = new int[]
        {
            0, 1, 3,
            0, 3, 2,
            2, 5, 4,
            2, 3, 5
        };

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        length_ = length;

        transform.LookAt(transform.position + forward, Vector3.up);
    }

    public List<Vector3> GetEnterTargets()
    {
        List<Vector3> targets = new List<Vector3>();

        Road parentRoad = GetComponentInParent<Road>();
        if(parentRoad)
        {
            //add the first target - point on road before parking
            targets.Add(parentRoad.GetPointOnRoad(transform.position - transform.forward*length_));
            //move the car into the spot
            //move the car further into the spot
            //targets.Add(transform.position);// + transform.forward * length_*0.25f);
            //align the car better
            //targets.Add(transform.position + transform.forward * length_ * 0.25f + transform.right * 0.5f);
            //targets.Add(transform.position - transform.forward * length_ * 0.25f);// - transform.right * 0.5f);
            targets.Add(transform.position + transform.forward * length_ * 0.5f);

            targets.Add(transform.position - transform.forward * length_ * 0.25f);// + transform.right*0.5f);

        }

        return targets;
    }

    public List<Vector3> GetExitTargets()
    {
        List<Vector3> targets = new List<Vector3>();

        Road parentRoad = GetComponentInParent<Road>();
        if (parentRoad)
        {
            //add the first target - make car reverse slightly
            targets.Add(transform.position - transform.forward * length_ * 0.25f);
            //move the car onto the road
            targets.Add(parentRoad.GetPointOnRoad(transform.position + transform.forward * length_));
        }

        return targets;
    }


    public Vector3 GetDirection()
    {
        return transform.forward;
    }
}
