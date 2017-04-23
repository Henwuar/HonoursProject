using UnityEngine;
using System.Collections;

public class DebugDisplayManager : MonoBehaviour
{
    [SerializeField]
    private GameObject personalityDisplay;

    private GameObject selectedCar_ = null;
    private bool stateTexts_ = true;
    private bool improvements_ = true;

	// Use this for initialization
	void Start ()
    {
        personalityDisplay.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(selectedCar_)
        {
            personalityDisplay.SetActive(true);
            UpdatePersonalityBars();
        }
        else
        {
            personalityDisplay.SetActive(false);
        }
	}

    void UpdatePersonalityBars()
    {
        Personality personality = selectedCar_.GetComponent<Personality>();
        float aggression = 0.0f;
        float defensiveness = 0.0f;
        float inattentiveness = 0.0f;

        if (personality)
        {
            aggression = personality.GetAggression();
            defensiveness = personality.GetDefensiveness();
            inattentiveness = personality.GetInattentiveness();
        }

        personalityDisplay.transform.FindChild("Aggression").GetComponent<UIProgressBar>().SetPercentage(aggression);
        personalityDisplay.transform.FindChild("Defensiveness").GetComponent<UIProgressBar>().SetPercentage(defensiveness);
        personalityDisplay.transform.FindChild("Inattentiveness").GetComponent<UIProgressBar>().SetPercentage(inattentiveness);
    }

    public void ToggleCarStates()
    {
        stateTexts_ = !stateTexts_;
        foreach (Car car in GameObject.Find("Cars").GetComponentsInChildren<Car>())
        {
            car.ToggleStateText(stateTexts_);
        }
    }

    public void ToggleImprovements()
    {
        improvements_ = !improvements_;
        foreach (Car car in GameObject.Find("Cars").GetComponentsInChildren<Car>())
        {
            car.ToggleImprovements(improvements_);
            if (!improvements_)
            {
                car.GetComponent<Personality>().ResetToBase();
            }
        }
    }

    public void SetSelectedCar(GameObject car)
    {
        selectedCar_ = car;
    }
}
