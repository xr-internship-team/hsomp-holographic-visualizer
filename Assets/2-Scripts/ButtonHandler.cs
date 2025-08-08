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

    public Interactable configureOffsetButton;

    public Interactable interpolationToggle;



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

        if (configureOffsetButton != null)
        {
            configureOffsetButton.OnClick.AddListener(ConfigureOffsetClicked);
        }

        if (interpolationToggle != null)
        {
            interpolationToggle.OnClick.AddListener(OnInterpolationToggleChanged);
        }

        timeSign = logger.GetTimeSign();
        smoothingSpeed = targetPositionUpdater.GetSmoothingSpeed();
        smoothLevelButtonText.text = "Smoothing Level: " + smoothingSpeed.ToString("F1"); ;
        TimeSignButtonText.text = "TimeSign: " + timeSign;
    }
    #endregion

    #region PrivateFunctions

    private void OnInterpolationToggleChanged()
    {
        bool isToggled = interpolationToggle.IsToggled;

        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.EnableInterpolation(isToggled);
            Debug.Log("Interpolation toggled: " + isToggled);
        }
    }

    private void ConfigureOffsetClicked()
    {
        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.ConfigureOffset();
            Debug.Log("Offset configuration triggered from button.");
        }
    }

    private void SmoothIncButtonClicked()
    {
        smoothingSpeed += 0.1f;
        smoothLevelButtonText.text = "Smoothing Level: " + smoothingSpeed.ToString("F1"); ;
        Debug.Log("Inc button clicked. smooth = " + smoothingSpeed.ToString("F1"));

        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.SetSmoothingSpeed(smoothingSpeed);
        }
    }
    private void SmoothDecButtonClicked()
    {
        smoothingSpeed -= 0.1f;
        smoothLevelButtonText.text = "Smoothing Level: " + smoothingSpeed.ToString("F1"); ;
        Debug.Log("Dec button clicked. smooth = " + smoothingSpeed.ToString("F1"));

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