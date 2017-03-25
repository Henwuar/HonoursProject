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

    private int stage_ = 0;
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
            else if(!running_)
            {
                useImprovements_ = false;
            }

            running_ = false;

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
            stage_ = 1;
            
            foreach(GameObject car in GameObject.FindGameObjectsWithTag("Car"))
            {
                car.GetComponent<Car>().ToggleImprovements(useImprovements_);
                car.GetComponent<Car>().ToggleStateText();
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
                events_.SetTrackData(true);
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
        print(checkpoints_.Progress());
        if(checkpoints_.Progress() >= 0.5f)
        {
            print("Half way");
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
        countDownText_.text = "FINISHED!";// + checkpoints_.GetTimerString();
        events_.SetTrackData(false);
        if(Input.GetButtonDown("Start"))
        {
            CleanUp();
        }
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
            car.GetComponent<Car>().ToggleStateText();
        }
    }

    public bool UseImprovements()
    {
        return useImprovements_;
    }
}
