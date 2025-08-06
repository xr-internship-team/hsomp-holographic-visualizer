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

    private double _lastAppliedTimestamp = 0;


    public Interactable smoothLevelButtonInc;
    public Interactable smoothLevelButtonDec;

    public TextMeshPro smoothLevelButtonText;

    #region UnityEventFunctions
    private void Start()
    {
        smoothLevelButtonInc.OnClick.AddListener(SmoothIncButtonClicked);
        smoothLevelButtonDec.OnClick.AddListener(SmoothDecButtonClicked);


        _receiver = new UdpReceiver(12345);
        RunThread();
    }

    private void Update()
    {
        if (_receivedDataQueue.Count > 0)
        {
            var receivedData = _receivedDataQueue.Dequeue();

            double incomingTimestamp = receivedData.GetTimeStamp();

            // Sýra dýþý gelen paketi atla
            if (incomingTimestamp > _lastAppliedTimestamp)
            {
                targetPositionUpdater.CubePositionSetter(receivedData.GetPosition(), receivedData.GetRotation());
                _lastAppliedTimestamp = incomingTimestamp;

                Debug.Log($"STAJ: Applied new data | Timestamp: {incomingTimestamp}");
            }
            else
            {
                Debug.LogWarning($"STAJ: Skipped outdated data | Incoming: {incomingTimestamp} < Last: {_lastAppliedTimestamp}");
            }
        }
    }

    private void SmoothIncButtonClicked()
    {
        targetPositionUpdater.smoothingSpeed += 1;
        smoothLevelButtonText.text = "Smoothing Level: " + targetPositionUpdater.smoothingSpeed;
        Debug.Log("Inc button clicked. smooth = " + targetPositionUpdater.smoothingSpeed);

    }
    private void SmoothDecButtonClicked()
    {
        targetPositionUpdater.smoothingSpeed -= 1;
        smoothLevelButtonText.text = "Smoothing Level: " + targetPositionUpdater.smoothingSpeed;
        Debug.Log("Dec button clicked. smooth = " + targetPositionUpdater.smoothingSpeed);

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
            _receivedDataQueue.Enqueue(data);
            Debug.Log("STAJ: Data received. | position: " + data.GetPosition() + " | rotation: " + data.GetRotation() + " | queue count: " + _receivedDataQueue.Count);
        }
    }
    
    #endregion
}
