using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

public class ButtonHandler : MonoBehaviour
{
    public Logger logger;

    public Interactable TimeSignButton;
    public TextMeshPro TimeSignButtonText;

    public TargetPositionUpdater targetPositionUpdater;

    public Interactable smoothLevelButtonInc;
    public Interactable smoothLevelButtonDec;
    public TextMeshPro smoothLevelButtonText;

    private int timeSign;
    private float smoothingSpeed;

    #region UnityEventFunctions
    // Start is called before the first frame update
    void Start()
    {
        if (TimeSignButton != null)
        {
            TimeSignButton.OnClick.AddListener(TimeSignButtonClicked);
        }

        if (targetPositionUpdater != null)
        {
            smoothLevelButtonInc.OnClick.AddListener(SmoothIncButtonClicked);
            smoothLevelButtonDec.OnClick.AddListener(SmoothDecButtonClicked);
        }

        timeSign = logger.GetTimeSign();
        smoothingSpeed = targetPositionUpdater.GetSmoothingSpeed();
        smoothLevelButtonText.text = "Smoothing Level: " + smoothingSpeed;
        TimeSignButtonText.text = "TimeSign: " + timeSign;
    }
    #endregion

    #region PrivateFunctions
    private void SmoothIncButtonClicked()
    {
        smoothingSpeed += 1;
        smoothLevelButtonText.text = "Smoothing Level: " + smoothingSpeed;
        Debug.Log("Dec button clicked. smooth = " + smoothingSpeed);

        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.SetSmoothingSpeed(smoothingSpeed);
        }
    }
    private void SmoothDecButtonClicked()
    {
        smoothingSpeed -= 1;
        smoothLevelButtonText.text = "Smoothing Level: " + smoothingSpeed;
        Debug.Log("Dec button clicked. smooth = " + smoothingSpeed);

        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.SetSmoothingSpeed(smoothingSpeed);
        }
    }

    private void TimeSignButtonClicked()
    {
        timeSign += 1;
        Debug.Log("Logger button clicked. TimeSign = " + timeSign);
        TimeSignButtonText.text = "TimeSign: " + timeSign;

        if (logger != null)
        {
            logger.SetTimeSign(timeSign);
        }
    }
    #endregion
}