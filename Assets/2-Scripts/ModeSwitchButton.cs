using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

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
        int nextMode = ((int)receiverProcessor.currentMode + 1) % 4;
        receiverProcessor.SetProcessingMode(nextMode);
        Debug.Log("Cycled to mode: " + (ReceiverProcessor.ProcessingMode)nextMode);
        modeText.text = $"Mode: {receiverProcessor.currentMode}";
    }
}