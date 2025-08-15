using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;

/// Handles all button interactions in the UI for controlling smoothing, offset configuration, 
/// interpolation toggle, and TimeSign updates.
public class ButtonHandler : MonoBehaviour
{
    [Header("Reference Files")]
    public Logger logger; // Reference to Logger for time sign updates
    public TargetPositionUpdater targetPositionUpdater;
    
    [Header("interactable Buttons")]
    public Interactable smoothLevelButtonInc;
    public Interactable smoothLevelButtonDec;
    public Interactable configureOffsetButton;
    public Interactable timeSignButton;
    public Interactable interpolationToggle;
    
    [Header("UI Texts")]
    public TextMeshPro smoothLevelButtonText;
    public TextMeshPro configureOffsetButtonText;
    public TextMeshPro timeSignButtonText;
    
    [Header("Smoothing Step Settings")]
    public float smoothingStep = 0.1f; // Increment/decrement step for smoothing, adjustable in Inspector

    private Coroutine _configureCoroutine = null; // Coroutine reference for countdown display

    private int _timeSign;                // Current time sign
    private float _smoothingSpeed;        // Current smoothing speed

    #region UnityEventFunctions

    /// Unity Start method. Adds listeners to buttons and initializes UI values.
    void Start()
    {
        if (timeSignButton != null)
        {
            timeSignButton.OnClick.AddListener(TimeSignButtonClicked);
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
        _timeSign = logger.GetTimeSign();
        _smoothingSpeed = targetPositionUpdater.GetSmoothingSpeed();

        smoothLevelButtonText.text = "Smoothing Level: " + _smoothingSpeed.ToString("F1"); ;
        timeSignButtonText.text = "TimeSign: " + _timeSign;
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

    /// Called when the configured offset button is clicked.
    /// Starts the offset configuration and countdown coroutine.
    private void ConfigureOffsetClicked()
    {
        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.ConfigureOffsetMultiple();

            if (_configureCoroutine != null)
                StopCoroutine(_configureCoroutine);

            // Calculate the total duration of the configuration
            var totalTime = targetPositionUpdater.configureInterval * targetPositionUpdater.configureSteps;
            _configureCoroutine = StartCoroutine(ConfigureOffsetCountdown(totalTime, targetPositionUpdater.configureSteps));

            Debug.Log("Offset configuration triggered from button.");
        }
    }

    /// Increase smoothing speed and update UI.
    private void SmoothIncButtonClicked()
    {
        _smoothingSpeed += smoothingStep;
        smoothLevelButtonText.text = "Smoothing Level: " + _smoothingSpeed.ToString("F1"); ;
        Debug.Log("Inc button clicked. smooth = " + _smoothingSpeed.ToString("F1"));

        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.SetSmoothingSpeed(_smoothingSpeed);
        }
    }

    /// Decrease smoothing speed and update UI.
    private void SmoothDecButtonClicked()
    {
        _smoothingSpeed -= smoothingStep;
        smoothLevelButtonText.text = "Smoothing Level: " + _smoothingSpeed.ToString("F1"); ;
        Debug.Log("Dec button clicked. smooth = " + _smoothingSpeed.ToString("F1"));

        if (targetPositionUpdater != null)
        {
            targetPositionUpdater.SetSmoothingSpeed(_smoothingSpeed);
        }
    }

    /// Increases the time sign and updates logger and UI.
    private void TimeSignButtonClicked()
    {
        _timeSign += 1;
        Debug.Log("Logger button clicked. TimeSign = " + _timeSign);
        timeSignButtonText.text = "TimeSign: " + _timeSign;

        if (logger != null)
        {
            logger.SetTimeSign(_timeSign);
        }
    }

    /// Countdown display coroutine for offset configuration.
    /// Updates the configureOffsetButtonText each interval.
    private IEnumerator ConfigureOffsetCountdown(float totalDuration, int steps)
    {
        var interval = totalDuration / steps;

        for (var i = steps; i > 0; i--)
        {
            configureOffsetButtonText.text = $"Align Object\n{i * interval:0.0}s";
            yield return new WaitForSeconds(interval);
        }

        configureOffsetButtonText.text = "Align Object\n";
    }
    #endregion
}