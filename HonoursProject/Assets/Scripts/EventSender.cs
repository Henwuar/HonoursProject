using UnityEngine;
using System.Collections;

public class EventSender : MonoBehaviour
{
    private EventTracker tracker_;
    [SerializeField]
    private TrafficEvent curEvent_;
    [SerializeField]
    private TrafficEvent prevEvent_;

	// Use this for initialization
	void Start ()
    {
        tracker_ = GameObject.FindGameObjectWithTag("EventTracker").GetComponent<EventTracker>();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(curEvent_ != prevEvent_ && curEvent_ != TrafficEvent.TE_NONE)
        {
            tracker_.AddEvent(curEvent_);
        }
        prevEvent_ = curEvent_;
	}

    public void SendEvent(TrafficEvent ev)
    {
        curEvent_ = ev;
    }

    public TrafficEvent CurEvent()
    {
        return curEvent_;
    }
}
