using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum TrafficEvent { TE_NONE = -1, TE_COLLISION = 0, TE_STALLED = 1, TE_DISTRACTED = 2, TE_PARKED = 3 };

public class EventTracker : MonoBehaviour
{
    public Text sessionText_;

    [SerializeField]
    private string outputPath_;
    [SerializeField]
    private bool trackData_;

    private float[] eventCount_;
    private InputField pathInput;
    private int sessionNumber_;

    // Use this for initialization
    void Start ()
    {
        if(PlayerPrefs.HasKey("output"))
        {
            outputPath_ = PlayerPrefs.GetString("output");
        }
        else
        {
            outputPath_ = "C:\\TrafficEvents";
        }
        if(PlayerPrefs.HasKey("session"))
        {
            sessionNumber_ = PlayerPrefs.GetInt("session") + 1;
        }
        else
        {
            sessionNumber_ = 0;
        }
        PlayerPrefs.SetInt("session", sessionNumber_);
        PlayerPrefs.Save();

        eventCount_ = new float[4];
        pathInput = transform.GetChild(0).GetComponent<InputField>();
        //set up the input field to pass its string to this object
        pathInput.onEndEdit.AddListener((input) => SetOutputPath(input));
        pathInput.text = outputPath_;
        pathInput.gameObject.SetActive(false);
        trackData_ = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            pathInput.gameObject.SetActive(true);
            Time.timeScale = 0.0f;            
        }

        if(Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            sessionNumber_++;
            PlayerPrefs.SetInt("session", sessionNumber_);
            PlayerPrefs.Save();
        }
        if(Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if(sessionNumber_ > 0)
            {
                sessionNumber_--;
                PlayerPrefs.SetInt("session", sessionNumber_);
                PlayerPrefs.Save();
            }
            
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (pathInput.gameObject.activeSelf)
            {
                pathInput.gameObject.SetActive(false);
                Time.timeScale = 1.0f;
            }
            else
            {
                Application.Quit();
            }
            
        }

        sessionText_.text = sessionNumber_.ToString();
    }

    public void WriteData(bool improved)
    {
        string eventFile = "";
        eventFile += "COLLISION:";
        eventFile += eventCount_[0].ToString() + "\n";
        eventFile += "STALLED:";
        eventFile += eventCount_[1].ToString() + "\n";
        eventFile += "DISTRACTED:";
        eventFile += eventCount_[2].ToString() + "\n";
        eventFile += "PARKED:";
        eventFile += eventCount_[3].ToString();

        System.DateTime now = System.DateTime.Now;
        string fname = sessionNumber_.ToString();//now.Year.ToString() + now.Month.ToString() + now.Day.ToString() + "_" + now.Hour.ToString() + now.Minute.ToString();
        if(improved)
        {
            fname += "_I";
        }
        System.IO.File.WriteAllText(outputPath_ + "\\TrafficEvents_" + fname + ".txt", eventFile);

        //reset the events;
        eventCount_ = new float[4];
    }

    public void AddEvent(TrafficEvent ev)
    {
        if(ev != TrafficEvent.TE_NONE && trackData_)
        {
            eventCount_[(int)ev]++;
        }
    }

    public void SetOutputPath(string path)
    {
        outputPath_ = path;
        pathInput.gameObject.SetActive(false);
        Time.timeScale = 1;
        //create the directory if necessary
        if(!System.IO.Directory.Exists(outputPath_))
        {
            System.IO.Directory.CreateDirectory(outputPath_);
        }
        PlayerPrefs.SetString("output", path);
        PlayerPrefs.Save();
    }

    public void toggleTrackData()
    {
        trackData_ = !trackData_;
    }

    public void SetTrackData(bool value)
    {
        trackData_ = value;
    }
}
