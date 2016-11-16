using UnityEngine;
using System.Collections;

public enum Signals { S_STOP, S_GO };

public class TrafficLight : MonoBehaviour
{
    private Signals currentSignal_ = Signals.S_STOP;
    public Material redLight_;
    public Material greenLight_;

	// Use this for initialization
	void Start ()
    {
        redLight_ = transform.FindChild("Red").GetComponent<Renderer>().material;
        greenLight_ = transform.FindChild("Green").GetComponent<Renderer>().material;
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(currentSignal_ == Signals.S_STOP)
        {
            redLight_.color = Color.red;
            greenLight_.color = Color.black;
        }
        else
        {
            redLight_.color = Color.black;
            greenLight_.color = Color.green;
        }
	}

    public Signals GetSignal()
    {
        return currentSignal_;
    }

    public void SetSignal(Signals signal)
    {
        currentSignal_ = signal;
    }
}
