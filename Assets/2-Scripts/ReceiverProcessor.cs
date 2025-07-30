using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ReceiverProcessor : MonoBehaviour
{
    public TargetPositionUpdater targetPositionUpdater;

    private IReceiver _receiver;
    private Thread _receiveThread;
    private Queue<ReceivedData> _receivedDataQueue = new();
    private readonly object _queueLock = new object();

    private DateTime _lastTimestamp = DateTime.MinValue;

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
                var receivedData = _receivedDataQueue.Peek();
                DateTime currentTimestamp = receivedData.GetTimestamp();

                if (currentTimestamp <= _lastTimestamp)
                {
                    _receivedDataQueue.Dequeue();
                    Debug.LogWarning("STAJ: Eski/tutarsız veri atlandı. Timestamp: " + currentTimestamp);
                    return;
                }

                _receivedDataQueue.Dequeue();
                _lastTimestamp = currentTimestamp;

                targetPositionUpdater.CubePositionSetter(
                    receivedData.GetPosition(),
                    receivedData.GetRotation()
                );

                Debug.Log("STAJ: Veri işlendi. Timestamp: " + currentTimestamp);
            }
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

    private void RunThread()
    {
        _receiveThread = new Thread(ReceiveThread);
        _receiveThread.IsBackground = true;
        _receiveThread.Start();
        Debug.Log("STAJ: UDP dinleme thread'i başlatıldı.");
    }

    private void ReceiveThread()
    {
        while (true)
        {
            var data = _receiver.GetData();
            if (data == null) continue;

            lock (_queueLock)
            {
                _receivedDataQueue.Enqueue(data);

                while (_receivedDataQueue.Count > 10)
                {
                    _receivedDataQueue.Dequeue();
                    Debug.LogWarning("STAJ: Fazla veri birikmişti, eski veri atıldı.");
                }
            }

            Debug.Log("STAJ: Veri alındı ve kuyruğa eklendi.");
        }
    }
}
