using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// Handles receiving UDP data on a separate thread and applies it to the target object.
public class ReceiverProcessor : MonoBehaviour
{
    [Header("Target Settings")]
    public TargetPositionUpdater targetPositionUpdater; // Reference to the cube updater script

    [Header("UDP Settings")]
    [Tooltip("Port number for receiving UDP data")]
    public int udpPort = 12345;

    [Tooltip("Maximum number of data packets to keep in the queue")]
    public int queueCapacity = 40;

    private IReceiver _receiver;                         // UDP receiver interface
    private Thread _receiveThread;                       // Background thread for receiving data
    private Queue<ReceivedData> _receivedDataQueue;      // Queue for incoming data

    private double _lastAppliedTimestamp = 0;            // Last applied data timestamp
    private readonly object _queueLock = new();          // Lock object for queue access
    private bool _isRunning = false;                     // Flag to control the receiving thread

    #region UnityEventFunctions
    private void Awake()
    {
        // Initialize queue with configured capacity
        _receivedDataQueue = new Queue<ReceivedData>(queueCapacity);
    }
    private void Start()
    {
        // Initialize UDP receiver and start the background thread
        _receiver = new UdpReceiver(udpPort);
        RunThread();
    }

    private void Update()
    {
        lock (_queueLock)
        {
            if (_receivedDataQueue.Count > 0)
            {
                var receivedData = _receivedDataQueue.Dequeue();

                var incomingTimestamp = receivedData.GetTimeStamp();

                // Skip outdated packets
                if (incomingTimestamp > _lastAppliedTimestamp)
                {
                    targetPositionUpdater.CubePositionSetter(receivedData.GetPosition(), receivedData.GetRotation());
                    Debug.Log($"Applied new data | Incoming: {incomingTimestamp}, Last: {_lastAppliedTimestamp}, Diff: {(incomingTimestamp - _lastAppliedTimestamp) * 1000.0:F2} ms");

                    _lastAppliedTimestamp = incomingTimestamp;

                }
                else
                {
                    Debug.LogWarning($"Skipped outdated data | Incoming: {incomingTimestamp} < Last: {_lastAppliedTimestamp}");
                }
            }
        }
    }

    private void OnDisable()
    {
        StopReceiverThread(); // Ensure thread stops when object is disabled
    }

    private void OnDestroy()
    {
        StopReceiverThread(); // Ensure thread stops when object is destroyed
    }
    #endregion

    #region PrivateFunctions

    /// Safely stops the receiver thread and closes the UDP client.
    private void StopReceiverThread()
    {
        _isRunning = false;
        _receiver.Close();

        if (_receiveThread != null && _receiveThread.IsAlive)
        {
            _receiveThread.Join(); // Wait for the thread to finish
        }

        StopAllCoroutines(); // Stop any coroutines if used
    }

    /// Starts the background thread for receiving UDP data.
    private void RunThread()
    {
        _isRunning = true;
        _receiveThread = new Thread(ReceiveThread);
        _receiveThread.IsBackground = true;
        _receiveThread.Start();
        Debug.Log("Thread ran.");
    }

    /// Thread loop to receive data and enqueue it for processing in Update().
    private void ReceiveThread()
    {
        while (_isRunning)
        {
            var data = _receiver.GetData();

            if (data != null)
            {
                lock (_queueLock)
                {
                    // Enqueue data; if queue exceeds capacity, remove oldest
                    if (_receivedDataQueue.Count >= queueCapacity)
                        _receivedDataQueue.Dequeue();

                    _receivedDataQueue.Enqueue(data);
                }

                Debug.Log("Data received. | position: " + data.GetPosition() +
                    " | rotation: " + data.GetRotation() +
                    " | queue count: " + _receivedDataQueue.Count);
            }
        }
    }
    #endregion
}
