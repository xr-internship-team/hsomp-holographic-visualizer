// SmoothFactorController.cs

using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

public class SmoothFactorController : MonoBehaviour
{
    public TargetPositionUpdater targetUpdater;

    public Interactable increaseButton;
    public Interactable decreaseButton;
    public TextMeshPro smoothCounterText;

    // Start with what you like to feel; 0.7 is a good default
    [Range(0f, 1f)]
    [SerializeField] private float _smoothFactor = 0.7f;

    private const float Step = 0.05f; // finer steps than 0.1
    private const float Min = 0.0f;
    private const float Max = 1.0f;

    private void Start()
    {
        increaseButton.OnClick.AddListener(IncreaseSmooth);
        decreaseButton.OnClick.AddListener(DecreaseSmooth);
        Apply();
    }

    private void IncreaseSmooth()
    {
        _smoothFactor = Mathf.Clamp(_smoothFactor + Step, Min, Max);
        Apply();
    }

    private void DecreaseSmooth()
    {
        _smoothFactor = Mathf.Clamp(_smoothFactor - Step, Min, Max);
        Apply();
    }

    private void Apply()
    {
        targetUpdater.SetSmoothFactor(_smoothFactor);
        smoothCounterText.text = $"Smooth Factor: {_smoothFactor:0.00}";
    }
}