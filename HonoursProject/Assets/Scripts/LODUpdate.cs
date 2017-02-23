using UnityEngine;
using System.Collections;

public class LODUpdate : MonoBehaviour
{
    [SerializeField]
    MonoBehaviour[] affectedComponents_;
    [SerializeField]
    private int frameSkip_;
    [SerializeField]
    private float maxUpdateDistance_;

    private int frameCount_ = 0;
    private bool prevVisibility_;

	// Use this for initialization
	void Start ()
    {
        prevVisibility_ = GetComponent<Renderer>().isVisible;
	}
	
	// Update is called once per frame
	void Update ()
    {
        //check how far away from the player/camera the object is
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 target = Camera.main.transform.position;
        if(player)
        {
            target = player.transform.position;
        }
        if (Vector3.Distance(transform.position, target) > maxUpdateDistance_)
        {
            foreach (MonoBehaviour component in affectedComponents_)
            {
                component.enabled = false;
            }
            GetComponent<Renderer>().enabled = false;
            return;
        }
        else
        {
            foreach (MonoBehaviour component in affectedComponents_)
            {
                component.enabled = true;
            }
            GetComponent<Renderer>().enabled = true;
        }

        frameCount_++;
        if (!GetComponent<Renderer>().isVisible)
        {
            if(frameCount_ > frameSkip_)
            {
                foreach (MonoBehaviour component in affectedComponents_)
                {
                    component.enabled = true;
                }
                frameCount_ = -1;
            }
            else if(frameCount_ == 0)
            {
                foreach (MonoBehaviour component in affectedComponents_)
                {
                    component.enabled = false;
                }
            }

        }
        else if(!prevVisibility_)
        {
            foreach(MonoBehaviour component in affectedComponents_)
            {
                component.enabled = true;
            }
        }

        prevVisibility_ = GetComponent<Renderer>().isVisible;
    }
}
