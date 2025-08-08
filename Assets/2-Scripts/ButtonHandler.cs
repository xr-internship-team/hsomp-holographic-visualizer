using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;

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
    public TextMeshPro configureOffsetButtonText;

    public Interactable interpolationToggle;

    private Coroutine configureCoroutine = null;




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
            targetPositionUpdater.ConfigureOffsetMultiple();

            if (configureCoroutine != null)
                StopCoroutine(configureCoroutine);

            // Toplam süre = interval * steps
            float totalTime = targetPositionUpdater.configureInterval * targetPositionUpdater.configureSteps;
            configureCoroutine = StartCoroutine(ConfigureOffsetCountdown(totalTime, targetPositionUpdater.configureSteps));

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

    private IEnumerator ConfigureOffsetCountdown(float totalDuration, int steps)
    {
        float interval = totalDuration / steps;

        for (int i = steps; i > 0; i--)
        {
            configureOffsetButtonText.text = $"Configure offset in:\n{i * interval:0.0}s\n\n";
            yield return new WaitForSeconds(interval);
        }

        configureOffsetButtonText.text = "Configure offset done.\nPress to configure again.\n\n\n";
    }
}