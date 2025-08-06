using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmoothFactorController : MonoBehaviour
{
    public TargetPositionUpdater targetUpdater;

    public Interactable increaseButton;
    public Interactable  decreaseButton;
    public TextMeshProUGUI smoothCounterText;  // Eğer Text kullandıysan: public Text counterText;

    private int _smoothLevel = 5; // 0-10 arası sayı
    private const int _minLevel = 0;
    private const int _maxLevel = 10;

    private void Start()
    {
        increaseButton.OnClick.AddListener(IncreaseSmooth);
        decreaseButton.OnClick.AddListener(DecreaseSmooth);
        UpdateUI();
    }

    private void IncreaseSmooth()
    {
        if (_smoothLevel < _maxLevel)
        {
            _smoothLevel++;
            UpdateSmoothFactor();
        }
    }

    private void DecreaseSmooth()
    {
        if (_smoothLevel > _minLevel)
        {
            _smoothLevel--;
            UpdateSmoothFactor();
        }
    }

    private void UpdateSmoothFactor()
    {
        // 0–10 arasında lineer ölçekle (örnek: 0.0f–1.0f)
        float mappedValue = _smoothLevel / 10f;
        targetUpdater.SetSmoothFactor(mappedValue);
        UpdateUI();
    }

    private void UpdateUI()
    {
        smoothCounterText.text = $"Smooth Level: {_smoothLevel}";
    }
}