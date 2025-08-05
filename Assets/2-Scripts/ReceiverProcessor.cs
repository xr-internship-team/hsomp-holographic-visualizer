using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ReceiverProcessor : MonoBehaviour
{
    public TargetPositionUpdater targetPositionUpdater;
    public Logger logger;

    private IReceiver _receiver;
    private Thread _receiveThread;
    private Queue<ReceivedData> _receivedDataQueue = new(40);
    
    private double _lastTimestamp; // epoch timestamp olarak

    #region UnityEventFunctions
    private void Start()
    {
        _receiver = new UdpReceiver(12345);
        RunThread();
    }

    private void Update()
    {
        lock (_receivedDataQueue)
        {
            if (_receivedDataQueue.Count > 0)
            {
                var receivedData = _receivedDataQueue.Dequeue();
                targetPositionUpdater.CubePositionSetter(receivedData.GetPosition(), receivedData.GetRotation());
            }
        }
        //Debug.Log("STAJ: Data dequeued. " + " | " + _receivedDataQueue.Count);
        
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
            double dataTime = data.timestamp; 
            // Debug.Log($"STAJ: Data received. Timestamp: {data.timestamp} | Epoch: {dataTime} | Pos: {data.GetPosition()} | Rot: {data.GetRotation()}");
            
            if (dataTime > _lastTimestamp) 
            {
                lock (_receivedDataQueue)
                {
                    _receivedDataQueue.Enqueue(data);
                }
                _lastTimestamp = dataTime;
                // Debug.Log($"STAJ: Accepted timestamp: {dataTime}");
            }
            else
            {
                Debug.LogWarning($"STAJ: Rejected outdated data. Timestamp: {dataTime}, Last: {_lastTimestamp}");
            }
        }
    }
    
    #endregion
}
