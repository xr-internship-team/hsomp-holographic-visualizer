using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;

public class ReceiverProcessor : MonoBehaviour
{
    public TargetPositionUpdater targetPositionUpdater;

    private IReceiver _receiver;
    private Thread _receiveThread;
    private Queue<ReceivedData> _receivedDataQueue = new(40);

    public Interactable smoothLevelButtonInc;
    public Interactable smoothLevelButtonDec;
    public TextMeshPro smoothLevelButtonText;

    #region UnityEventFunctions
    private void Start()
    {
        smoothLevelButtonInc.OnClick.AddListener(SmoothIncButtonClicked);
        smoothLevelButtonDec.OnClick.AddListener(SmoothDecButtonClicked);

        // Initialize button text to show the interpolation delay
        if (targetPositionUpdater != null)
        {
            smoothLevelButtonText.text = $"Delay: {targetPositionUpdater.interpolationDelay * 1000:F0} ms";
        }

        _receiver = new UdpReceiver(12345);
        RunThread();
    }

    private void Update()
    {
        if (_receivedDataQueue.Count > 0)
        {
            var receivedData = _receivedDataQueue.Dequeue();
            // Pass the entire data object to the new handler in TargetPositionUpdater
            targetPositionUpdater.OnDataReceived(receivedData);
        }
    }

    private void SmoothIncButtonClicked()
    {
        if (targetPositionUpdater == null) return;
        // Increase delay by 10ms
        targetPositionUpdater.interpolationDelay += 0.01f;
        smoothLevelButtonText.text = $"Delay: {targetPositionUpdater.interpolationDelay * 1000:F0} ms";
        Debug.Log("Inc button clicked. delay = " + targetPositionUpdater.interpolationDelay);
    }

    private void SmoothDecButtonClicked()
    {
        if (targetPositionUpdater == null) return;
        // Decrease delay by 10ms, with a minimum of 0
        targetPositionUpdater.interpolationDelay = Mathf.Max(0, targetPositionUpdater.interpolationDelay - 0.01f);
        smoothLevelButtonText.text = $"Delay: {targetPositionUpdater.interpolationDelay * 1000:F0} ms";
        Debug.Log("Dec button clicked. delay = " + targetPositionUpdater.interpolationDelay);
    }

    private void OnDisable()
    {
        _receiver.Close();
        _receiveThread?.Abort();
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        _receiver.Close();
        _receiveThread?.Abort();
        StopAllCoroutines();
    }
    
    #endregion

    #region PrivateFunctions

    private void RunThread()
    {
        _receiveThread = new Thread(ReceiveThread);
        _receiveThread.IsBackground = true;
        _receiveThread.Start();
        Debug.Log("STAJ: Thread ran.");
    }
    
    private void ReceiveThread()
    {
        while (true)
        {
            var data = _receiver.GetData();
            if (data != null)
            {
                _receivedDataQueue.Enqueue(data);
            }
        }
    }
    
    #endregion
}
