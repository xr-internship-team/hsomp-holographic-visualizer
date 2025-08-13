using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;

/// Handles all button interactions in the UI for controlling smoothing, offset configuration, 
/// interpolation toggle, and TimeSign updates.
public class ButtonHandler : MonoBehaviour
{
    public Logger logger; // Reference to Logger for time sign updates

    public Interactable TimeSignButton;
    public TextMeshPro TimeSignButtonText;

    public TargetPositionUpdater targetPositionUpdater;

    public Interactable smoothLevelButtonInc;
    public Interactable smoothLevelButtonDec;
    public TextMeshPro smoothLevelButtonText;

    public Interactable configureOffsetButton;
    public TextMeshPro configureOffsetButtonText;

    public Interactable interpolationToggle;

    [Header("Smoothing Step Settings")]
    public float smoothingStep = 0.1f; // Increment/decrement step for smoothing, adjustable in Inspector

    private Coroutine configureCoroutine = null; // Coroutine reference for countdown display

    private int timeSign;                // Current time sign
    private float smoothingSpeed;        // Current smoothing speed

    #region UnityEventFunctions

    /// Unity Start method. Adds listeners to buttons and initializes UI values.
    void Start()
    {
        if (TimeSignButton != null)
        {
            TimeSignButton.OnClick.AddListener(TimeSignButtonClicked);
        }

        if (targetPositionUpdater != null)
        {
            if (smoothLevelButtonInc != null)
                smoothLevelButtonInc.OnClick.AddListener(SmoothIncButtonClicked);

            if (smoothLevelButtonDec != null)
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

        // Initialize values from Logger and TargetPositionUpdater
        timeSign = logger.GetTimeSign();
        smoothingSpeed = targetPositionUpdater.GetSmoothingSpeed();

        smoothLevelButtonText.text = "Smoothing Level: " + smoothingSpeed.ToString("F1"); ;
        TimeSignButtonText.text = "TimeSign: " + timeSign;
    }
    #endregion

    #region PrivateFunctions

    /// Called when the interpolation toggle is changed.
    /// Enables/disables interpolation in TargetPositionUpdater.
    private void OnInterpolationToggleChanged()
    {
        bool isToggled = interpolationToggle.IsToggled;

        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.EnableInterpolation(isToggled);
            Debug.Log("Interpolation toggled: " + isToggled);
        }
    }

    /// Called when the configure offset button is clicked.
    /// Starts the offset configuration and countdown coroutine.
    private void ConfigureOffsetClicked()
    {
        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.ConfigureOffsetMultiple();

            if (configureCoroutine != null)
                StopCoroutine(configureCoroutine);

            // Calculate total duration of the configuration
            float totalTime = targetPositionUpdater.configureInterval * targetPositionUpdater.configureSteps;
            configureCoroutine = StartCoroutine(ConfigureOffsetCountdown(totalTime, targetPositionUpdater.configureSteps));

            Debug.Log("Offset configuration triggered from button.");
        }
    }

    /// Increase smoothing speed and update UI.
    private void SmoothIncButtonClicked()
    {
        smoothingSpeed += smoothingStep;
        smoothLevelButtonText.text = "Smoothing Level: " + smoothingSpeed.ToString("F1"); ;
        Debug.Log("Inc button clicked. smooth = " + smoothingSpeed.ToString("F1"));

        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.SetSmoothingSpeed(smoothingSpeed);
        }
    }

    /// Decrease smoothing speed and update UI.
    private void SmoothDecButtonClicked()
    {
        smoothingSpeed -= smoothingStep;
        smoothLevelButtonText.text = "Smoothing Level: " + smoothingSpeed.ToString("F1"); ;
        Debug.Log("Dec button clicked. smooth = " + smoothingSpeed.ToString("F1"));

        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.SetSmoothingSpeed(smoothingSpeed);
        }
    }

    /// Increases the time sign and updates logger and UI.
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

    /// Countdown display coroutine for offset configuration.
    /// Updates the configureOffsetButtonText each interval.
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
    #endregion
}