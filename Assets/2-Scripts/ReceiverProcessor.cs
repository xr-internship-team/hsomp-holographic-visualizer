using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ReceiverProcessor : MonoBehaviour
{
    public TargetPositionUpdater targetPositionUpdater;

    private IReceiver _receiver;
    private Thread _receiveThread;
    private Queue<ReceivedData> _receivedDataQueue = new(40);

    private double _lastAppliedTimestamp = 0;

    private readonly object _queueLock = new();

    private bool _isRunning = false;

    #region UnityEventFunctions
    private void Start()
    {
        _receiver = new UdpReceiver(12345);
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

                // Sýra dýþý gelen paketi atla
                if (incomingTimestamp > _lastAppliedTimestamp)
                {
                    targetPositionUpdater.CubePositionSetter(receivedData.GetPosition(), receivedData.GetRotation());
                    Debug.Log($"STAJ: Applied new data | Incoming: {incomingTimestamp}, Last: {_lastAppliedTimestamp}, Diff: {(incomingTimestamp - _lastAppliedTimestamp) * 1000.0:F2} ms");

                    _lastAppliedTimestamp = incomingTimestamp;

                }
                else
                {
                    Debug.LogWarning($"STAJ: Skipped outdated data | Incoming: {incomingTimestamp} < Last: {_lastAppliedTimestamp}");
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
