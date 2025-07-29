using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DataProcessor : MonoBehaviour
{
    public GameObject outsideCamera;
    public GameObject marker;
    public TargetPositionUpdater targetPositionUpdater;
    public IReceiver receiver;

    private Queue<ReceivedData> _receivedDataList = new(40);


    public GameObject testCube;
    private bool _isRunning;
    private Thread _receiveThread;

    void Start()
    {
        receiver = new UdpReceiver("0.0.0.0", 12345);
        receiver.CreateClient();
        StartReceiver();

    }

    private void Update()
    {
        if (_receivedDataList.Count > 0)
        {
            var receivedData = _receivedDataList.Dequeue();
            targetPositionUpdater.CubePositionSetter(receivedData.GetPosition(), receivedData.GetRotation());

        }

    }

    public void StartReceiver()
    {
        if (_receiveThread == null || !_receiveThread.IsAlive)
        {
            _isRunning = true;
            _receiveThread = new Thread(ReceiverThread);
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        }
    }

    public void StopReceiver()
    {
        _isRunning = false;
        if (_receiveThread != null && _receiveThread.IsAlive)
            _receiveThread.Join();
    }

    public void ReceiverThread()
    {
        while (_isRunning)
        {
            var data = receiver.GetData();
            _receivedDataList.Enqueue(data);

        }
    }
    
    private void OnDisable()
    {
        receiver.CloseClient();
        StopReceiver();
    }


}
