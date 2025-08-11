using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ReceiverProcessor : MonoBehaviour
{
    public TargetPositionUpdater targetPositionUpdater;
    private CameraHistoryRecorder _cameraHistory;


    private IReceiver _receiver;
    private Thread _receiveThread;
    private Queue<ReceivedData> _receivedDataQueue = new(40);


    private readonly object _queueLock = new();

    private bool _isRunning = false;

    #region UnityEventFunctions
    private void Start()
    {
        _receiver = new UdpReceiver(12345);
        _cameraHistory = FindObjectOfType<CameraHistoryRecorder>();


        RunThread();
    }

    private void Update()
    {
        lock (_queueLock)
        {
            if (_receivedDataQueue.Count > 0)
            {
                var receivedData = _receivedDataQueue.Dequeue();
                double incomingTimestamp = receivedData.GetTimeStamp();

                var closestSnapshot = _cameraHistory.GetClosestSnapshot(incomingTimestamp);
                if (closestSnapshot.HasValue)
                {
                    var snap = closestSnapshot.Value;

                    targetPositionUpdater.CubePositionSetter(
                        receivedData.GetPosition(),
                        receivedData.GetRotation(),
                        snap.position,
                        snap.rotation
                    );

                    Debug.Log("Received data's timestamp: " + incomingTimestamp +
                              " | Closest Snapshot's timestamp: " + snap.timestamp +
                              " | Dif (ms): " + ((incomingTimestamp - snap.timestamp) * 1000).ToString("F3"));
                }
            }
        }
    }

    private void OnDisable()
    {
        StopReceiverThread();
    }

    private void OnDestroy()
    {
        StopReceiverThread();
    }
    #endregion

    #region PrivateFunctions
    private void StopReceiverThread()
    {
        _isRunning = false;
        _receiver.Close();

        if (_receiveThread != null && _receiveThread.IsAlive)
        {
            _receiveThread.Join();
        }

        StopAllCoroutines();
    }
    private void RunThread()
    {
        _isRunning = true;
        _receiveThread = new Thread(ReceiveThread);
        _receiveThread.IsBackground = true;
        _receiveThread.Start();
        Debug.Log("STAJ: Thread ran.");
    }

    private void ReceiveThread()
    {
        while (_isRunning)
        {
            var data = _receiver.GetData();

            if (data != null)
            {
                lock (_queueLock)
                {
                    _receivedDataQueue.Enqueue(data);
                }

                Debug.Log("STAJ: Data received. | position: " + data.GetPosition() + " | rotation: " + data.GetRotation() + " | queue count: " + _receivedDataQueue.Count);
            }
        }
    }

    #endregion
}
