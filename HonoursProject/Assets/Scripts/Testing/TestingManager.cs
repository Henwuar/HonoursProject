using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum TestingType { TT_AB, TT_BOTH, TT_DYNAMIC};

public class TestingManager : MonoBehaviour
{
    public GameObject carPrefab_;
    public CheckpointManager checkpoints_;
    public EventTracker events_;

    [SerializeField]
    private TestingType test_;
    [SerializeField]
    private bool useImprovements_;
    [SerializeField]
    private Text countDownText_;
    [SerializeField]
    private bool trackEvents_ = false;

    private int stage_ = 0;
    private int phase2Stage_ = 0;
    private int curRun_ = 0;
    private bool running_;

    private GameObject car_;
    private Car carComponent_;

    private float countIn_ = 4.0f;
    private float timer_;

    // Use this for initialization
    void Start ()
    {
        timer_ = countIn_;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(Input.GetKeyDown(KeyCode.Delete))
        {
            CleanUp();
        }
        switch (stage_)
        {
            case 0: Stage0(); break;
            case 1: Stage1(); break;
            case 2: Stage2(); break;
            case 3: Stage3(); break;
            case 4: Stage4(); break;
            case 5: Stage5(); break;
            case 6: Stage6(); break;
            default: CleanUp(); break;
        }
	}

    void Stage0()
    {
        if (Input.GetButtonDown("Start"))
        {
            //set up the testing
            if (test_ == TestingType.TT_AB)
            {
                if(PlayerPrefs.HasKey("PrevImprovements"))
                {
                    if(PlayerPrefs.GetInt("PrevImprovements") == 0)
                    {
                        useImprovements_ = true;
                        PlayerPrefs.SetInt("PrevImprovements", 1);
                    }
                    else
                    {
                        useImprovements_ = false;
                        PlayerPrefs.SetInt("PrevImprovements", 0);
                    }
                }
                else
                {
                    useImprovements_ = false;
                    PlayerPrefs.SetInt("PrevImprovements", 0);
                }
                PlayerPrefs.Save();
            }

            if (test_ == TestingType.TT_BOTH)
            {
                useImprovements_ = (curRun_ == 1);
            }

            if(test_ == TestingType.TT_DYNAMIC)
            {
                useImprovements_ = false;
            }

            running_ = false;

            SpawnPlayerCar();
            stage_ = 1;
            
            foreach(GameObject car in GameObject.FindGameObjectsWithTag("Car"))
            {
                car.GetComponent<Car>().ToggleImprovements(useImprovements_);
                car.GetComponent<Car>().ToggleStateText(false);
            }
        }
    }

    void Stage1()
    {
        carComponent_.enabled = false;
        if (Input.GetButtonDown("Start"))
        {
            running_ = true;
        }
        if(running_)
        {

            timer_ -= Time.deltaTime;
            countDownText_.text = (Mathf.FloorToInt(timer_).ToString());
            if (Mathf.FloorToInt(timer_) <= 0)
            {
                countDownText_.text = "GO!";
                checkpoints_.StartTimer();
                car_.GetComponent<Car>().enabled = true;
                events_.SetTrackData(trackEvents_);
            }
            if(timer_ <= 0)
            {
                stage_ = 2;
                countDownText_.CrossFadeAlpha(0, 1, true);
            }
        }
    }

    void Stage2()
    {
        if(checkpoints_.Progress() >= 0.5f && test_ == TestingType.TT_DYNAMIC)
        {
            //print("Half way");
            foreach(GameObject car in GameObject.FindGameObjectsWithTag("Car"))
            {
                car.GetComponent<Car>().ToggleImprovements(true);
            }
        }

        carComponent_.enabled = true;
        if (checkpoints_.Complete())
        {
            stage_ = 3;
        }
    }

    void Stage3()
    {
        countDownText_.CrossFadeAlpha(1, 0.5f, true);
        if (test_ == TestingType.TT_DYNAMIC)
        {
            countDownText_.text = "FINISHED\nPlease complete SECTION 1 of the questionnaire";
            events_.SetTrackData(false);
            if(Input.GetButtonDown("Start"))
            {
                CleanUp();
                stage_ = 4;
                SpawnPlayerCar();
                foreach (GameObject car in GameObject.FindGameObjectsWithTag("Car"))
                {
                    car.GetComponent<Car>().ToggleStateText(false);
                }
            }
        }
        else
        {
            countDownText_.text = "FINISHED!";// + checkpoints_.GetTimerString();
            if(test_ == TestingType.TT_BOTH)
            {
                countDownText_.text += "\nPlease complete part " + (curRun_ + 1).ToString() + " of the questionnaire.";
            }
            events_.SetTrackData(false);

            if (Input.GetButtonDown("Start"))
            {
                CleanUp();
            }
        }
    }

    void Stage4()
    {
        Stage1();
        //check if stage1 has complete
        if(stage_ == 2)
        {
            Camera.main.GetComponent<CameraController>().ToggleCompass(false);

            stage_ = 5;
            checkpoints_.StartTimer(60.0f);
            if(phase2Stage_ == 0)
            {
                //set up totally defensive cars
                foreach(GameObject car in GameObject.FindGameObjectsWithTag("Car"))
                {
                    Personality personality = car.GetComponent<Personality>();
                    if(personality)
                    {
                        personality.Init(true, 0.0f, 1.0f, 0.0f);
                    }
                }
            }
            else if(phase2Stage_ == 1)
            {
                //set up totally aggressive cars
                foreach (GameObject car in GameObject.FindGameObjectsWithTag("Car"))
                {
                    Personality personality = car.GetComponent<Personality>();
                    if (personality)
                    {
                        personality.Init(true, 1.0f, 0.0f, 0.0f);
                    }
                }
            }
            else if(phase2Stage_ == 2)
            {
                //set up totally inattentive cars
                foreach (GameObject car in GameObject.FindGameObjectsWithTag("Car"))
                {
                    Personality personality = car.GetComponent<Personality>();
                    if (personality)
                    {
                        personality.Init(true, 0.0f, 0.0f, 1.0f);
                    }
                }
            }

        }
    }

    void Stage5()
    {
        carComponent_.enabled = true;
        if(checkpoints_.Complete())
        {
            stage_ = 6;
        }
    }

    void Stage6()
    {
        print("Stage 6");
        countDownText_.CrossFadeAlpha(1, 0.5f, true);
        countDownText_.text = "FINISHED\nPlease complete SECTION " + (phase2Stage_ + 2).ToString() +  " of the questionnaire";
        events_.SetTrackData(false);
        if (Input.GetButtonDown("Start"))
        {
            CleanUp();
            SpawnPlayerCar();

            foreach (GameObject car in GameObject.FindGameObjectsWithTag("Car"))
            {
                car.GetComponent<Car>().ToggleStateText(false);
            }
            stage_ = 4;
            phase2Stage_++;
            if(phase2Stage_ > 2)
            {
                stage_ = 7;
            }
        }
    }

    void Stage7()
    {
        if(Input.GetButtonDown("Start"))
        {
            CleanUp();
            checkpoints_.Reset();
            phase2Stage_ = 0;
        }
    }

    void SpawnPlayerCar()
    {
        Vector3 startPos = checkpoints_.GetStart() + Vector3.up;
        car_ = (GameObject)Instantiate(carPrefab_, startPos, Quaternion.identity);
        car_.GetComponent<Car>().Init();
        carComponent_ = car_.GetComponent<Car>();
        car_.GetComponent<Car>().ToggleControlled();
        car_.GetComponent<Car>().Invoke("Start", 0.0f);
        car_.GetComponent<Vision>().enabled = false;
        car_.GetComponent<Personality>().enabled = false;
        car_.GetComponent<Error>().enabled = false;
        car_.GetComponent<Purpose>().enabled = false;
        car_.tag = "Player";
        car_.transform.LookAt(checkpoints_.GetCheckpoint(1));
        Vector3 carRotation = car_.transform.eulerAngles;
        carRotation.y = Mathf.Round(carRotation.y / 90) * 90;
        car_.transform.eulerAngles = carRotation;
    }

    void CleanUp()
    {
        if(car_)
        {
            Destroy(car_);
        }
        running_ = false;
        timer_ = countIn_;
        checkpoints_.Reset();
        countDownText_.color = Color.black;
        countDownText_.text = "";

        events_.WriteData(useImprovements_);

        if(test_ == TestingType.TT_BOTH)
        {
            useImprovements_ = true;
            running_ = true;
        }
        stage_ = 0;

        foreach (GameObject car in GameObject.FindGameObjectsWithTag("Car"))
        { 
            car.GetComponent<Car>().ToggleStateText(true);
        }

        Camera.main.GetComponent<CameraController>().ToggleCompass(true);

        curRun_++;
        if(curRun_ > 1)
        {
            curRun_ = 0;
        }
    }

    public bool UseImprovements()
    {
        return useImprovements_;
    }
}
