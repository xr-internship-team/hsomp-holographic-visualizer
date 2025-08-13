using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;
using System;

public class ModeSwitchButton : MonoBehaviour
{
    public ReceiverProcessor receiverProcessor;
    public Interactable interactable;
    public TextMeshPro modeText;

    private void Start()
    {
        interactable.OnClick.AddListener(CycleMode);
        modeText.text = $"Mode: {receiverProcessor.currentMode}";
    }

    private void CycleMode()
    {
        int modeCount = Enum.GetValues(typeof(ReceiverProcessor.ProcessingMode)).Length;
        int next = ((int)receiverProcessor.currentMode + 1) % modeCount;
        receiverProcessor.SetProcessingMode(next);
        modeText.text = $"Mode: {receiverProcessor.currentMode}";
        Debug.Log("Cycled to mode: " + receiverProcessor.currentMode);
    }
}