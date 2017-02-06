using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParkingSpace : MonoBehaviour
{
    [SerializeField]
    private bool available_;
    private float length_;

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

    public void Init(float length, float width)
    {
        Vector3[] vertices = new Vector3[6];
        Vector2[] uvs = new Vector2[6];
        int[] triangles;

        //tl
        vertices[0] = transform.forward * length * 0.5f - transform.right*width*0.25f;
        //tr
        vertices[1] = transform.forward * length * 0.5f + transform.right * width * 0.25f;
        //ml
        vertices[2] = transform.right * width * 0.25f;
        //mr
        vertices[3] = transform.right * width * 0.25f;
        //bl
        vertices[4] = transform.forward * length * 0.5f - transform.right * width * 0.25f;
        //br
        vertices[5] = transform.forward * length * 0.5f + transform.right * width * 0.25f;
        
        uvs[0] = new Vector2(0, 1);
        uvs[1] = new Vector2(1, 1);
        uvs[2] = Vector2.zero;
        uvs[3] = new Vector2(1, 0);
        uvs[4] = new Vector2(0, 1);
        uvs[5] = new Vector2(1, 1);

        triangles = new int[]
        {
            0, 1, 3,
            0, 3, 2,
            2, 4, 5,
            2, 5, 3
        };

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        length_ = length;
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
            targets.Add(transform.position - transform.forward * length_*0.25f);
            //add the final stopping place
            targets.Add(transform.position);
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

}
