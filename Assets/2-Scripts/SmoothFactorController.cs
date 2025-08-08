using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmoothFactorController : MonoBehaviour
{
    public TargetPositionUpdater targetUpdater;

    public Interactable increaseButton;
    public Interactable  decreaseButton;
    public TextMeshPro smoothCounterText;

    private int _smoothLevel = 10;
    private const int MinLevel = 0;
    private const int MaxLevel = 20;

    private void Start()
    {
        increaseButton.OnClick.AddListener(IncreaseSmooth);
        decreaseButton.OnClick.AddListener(DecreaseSmooth);
        UpdateUI();
        UpdateSmoothFactor();
    }

    private void IncreaseSmooth()
    {
        if (_smoothLevel >= MaxLevel) return;
        _smoothLevel++;
        UpdateSmoothFactor();
    }

    private void DecreaseSmooth()
    {
        if (_smoothLevel <= MinLevel) return;
        _smoothLevel--;
        UpdateSmoothFactor();
    }

    private void UpdateSmoothFactor()
    {
        var mappedValue = _smoothLevel / 10f;
        targetUpdater.SetSmoothFactor(mappedValue);
        UpdateUI();
    }

    private void UpdateUI()
    {
        smoothCounterText.text = $"Smooth Level: {_smoothLevel}";
    }
}