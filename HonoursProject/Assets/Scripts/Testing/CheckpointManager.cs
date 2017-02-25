using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class CheckpointManager : MonoBehaviour
{
    public Text clock_;

    [SerializeField]
    private bool random_;
    [SerializeField]
    private Queue<Vector3> checkpoints_;
    [SerializeField]
    private int numCheckpoints_;
    [SerializeField]
    private int[] junctionIndices_;

    private float timer_ = 0.0f;
    private bool timing_ = false;

    // Use this for initialization
	void Start ()
    {
        checkpoints_ = new Queue<Vector3>();
        numCheckpoints_ = junctionIndices_.Length;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(timing_)
        {
            timer_ += Time.deltaTime;

            //set up the string to display
            string clockText = "";
            int milliseconds = (int)((timer_ - Mathf.Floor(timer_)) * 100);
            int seconds = Mathf.FloorToInt(timer_) % 60;// + milliseconds;
            int minutes = Mathf.FloorToInt(timer_ / 60);
            clockText = minutes.ToString("D2") + ":" + seconds.ToString("D2") + "." + milliseconds.ToString("D2");
            //apply the string
            clock_.text = clockText;
        }
	}

    public void Init(int cityWidth, int cityHeight)
    {
        //make sure start has been called
        Start();

        //randomly generate a series of checkpoints
        if(random_)
        {
            numCheckpoints_ = Random.Range(5, 10);
            junctionIndices_ = new int[numCheckpoints_];
            for(int index = 0; index < numCheckpoints_; index++)
            {
                junctionIndices_[index] = Random.Range(0, (cityWidth * cityHeight)-1);
            }
        }
        //get all the junctions in the scene
        GameObject[] junctions = GameObject.FindGameObjectsWithTag("Junction");
        for(int checkpoint = 0; checkpoint < numCheckpoints_; checkpoint++)
        {
            //get the current junction index
            int index = junctionIndices_[checkpoint];
            //make sure the junction is in range
            if(index < (cityWidth * cityHeight))
            { 
                //add the position to the queue
                GameObject junction = junctions[index];
                checkpoints_.Enqueue(junction.transform.position);
            }
        }

        transform.parent.position = checkpoints_.Peek();
    }

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            //get a new checkpoint
            if (checkpoints_.Count > 0)
            {
                transform.parent.position = checkpoints_.Dequeue();
            }
            else
            {
                timing_ = false;
                print("DONE: " + timer_);
            }
        }
        
    }

    public void StartTimer()
    {
        timing_ = true;
    }

    public void StopTimer()
    {
        timing_ = false;
    }
}
