using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ReceiverProcessor : MonoBehaviour
{
    public TargetPositionUpdater targetPositionUpdater;

    private IReceiver _receiver;
    private Thread _receiveThread;
    private Queue<ReceivedData> _receivedDataQueue = new(20);
    
    #region UnityEventFunctions
    private void Start()
    {
        _receiver = new UdpReceiver(12345);
        RunThread();
    }

    private void Update()
    {
        if (_receivedDataQueue.Count > 0)
        {
            var receivedData = _receivedDataQueue.Dequeue();
            targetPositionUpdater.CubePositionSetter(receivedData.GetPosition(),receivedData.GetRotation());
        }
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
    }
    
    private void ReceiveThread()
    {
        while (true)
        {
            var data = _receiver.GetData();
            _receivedDataQueue.Enqueue(data);
        }
    }
    
    #endregion
}
