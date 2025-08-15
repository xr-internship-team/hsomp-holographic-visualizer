using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine;

/// Logs tracked object, reference object, and camera positions/rotations along with distance metrics.
/// Uses a separate thread to write data to a CSV file asynchronously to avoid blocking the main thread.
public class Logger : MonoBehaviour
{
    public GameObject trackedObject;      // The main object being tracked
    public Transform playspaceTransform;  // Camera or user reference transform
    public GameObject refObject;          // Optional reference object for distance/rotation comparison

    [Header("Logging Settings")]
    [Tooltip("Enable or disable logging at runtime")]
    [SerializeField] private bool loggingEnabled = true;

    [Tooltip("Time interval (seconds) between log entries")]
    [SerializeField] private float logIntervalSecond = 0.1f;

    [Tooltip("Sleep time (ms) for the logging thread to reduce CPU usage")]
    [SerializeField] private int logThreadSleepMs = 10;

    private string filePath;              // Path to the CSV log file
    private StreamWriter writer;          // Stream writer to write log entries

    private float timeSinceLastLog = 0f;
    private bool loggingStarted = false;
    private float initialDistance = 0f;   // Distance between trackedObject and playspace at start
    private int timeSign = 0;             // External marker or counter for log synchronization

    private Thread logThread;             // Thread for asynchronous logging
    private bool isLoggingThreadRunning = true;

    private ConcurrentQueue<LogDataEntry> logQueue = new ConcurrentQueue<LogDataEntry>();

    // Struct for storing one log entry
    private struct LogDataEntry
    {
        public float timestamp;
        public Vector3 objPos;
        public Quaternion objRot;
        public Vector3 refPos;
        public Quaternion refRot;
        public Vector3 camPos;
        public Quaternion camRot;
        public float initialDistance;
        public float currentDistance;
        public float changeInDistance;
        public float refToTrackedDistance;
        public float rotationDifference;
        public int timeSign;
    }

    #region UnityEventFunctions

    /// Initializes logger, opens file, calculates initial distance, and starts the logging thread.
    void Start()
    {
        if (!loggingEnabled)
        {
            Debug.Log("Logger is disabled. No logging will occur.");
            return;
        }

        try
        {
            filePath = Path.Combine(Application.persistentDataPath, "DistanceLog.csv");
            Debug.Log("Logger path: " + filePath);

            // Open the file for writing (overwrite existing)
            writer = new StreamWriter(filePath, false);

            // Write CSV header
            writer.WriteLine("Time;ObjectX;ObjectY;ObjectZ;ObjectRotX;ObjectRotY;ObjectRotZ;ObjectRotW;" +
                             "RefX;RefY;RefZ;RefRotX;RefRotY;RefRotZ;RefRotW;" +
                             "CameraX;CameraY;CameraZ;CameraRotX;CameraRotY;CameraRotZ;CameraRotW;" +
                             "InitialDistance;CurrentDistance;ChangeInDistance;RefToTrackedObjDistance;RefToTrackedObjRotationDiff;TimeSign");
           
            // Calculate initial distance if objects are assigned
            if (trackedObject != null && playspaceTransform != null)
            {
                initialDistance = Vector3.Distance(trackedObject.transform.position, playspaceTransform.position);
            }

            loggingStarted = true;

            // Start logging thread
            logThread = new Thread(WriteLogThread);
            logThread.Start();

            Debug.Log("Logger started.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Logger init error: " + ex.Message);
        }
    }

    /// Adds new log data to the queue at fixed intervals.
    void Update()
    {
        if (!loggingStarted) return;

        timeSinceLastLog += Time.deltaTime;

        if (timeSinceLastLog >= logIntervalSecond)
        {
            EnqueueLogData();
            timeSinceLastLog = 0f;
        }
    }

    /// Stop logging thread and close the file when application quits.
    private void OnApplicationQuit()
    {
        if (!loggingEnabled || !loggingStarted) return;
        
        isLoggingThreadRunning = false;
        logThread?.Join(); // Wait for thread to finish

        try
        {
            writer?.Close();
            Debug.Log("Logger file closed.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Logger file close error: " + ex.Message);
        }
    }
    #endregion

    #region PrivateFunctions

    /// Collects current positions/rotations and distance metrics and enqueue them for logging.
    private void EnqueueLogData()
    {
        if (trackedObject == null || playspaceTransform == null) return;

        LogDataEntry entry = new LogDataEntry
        {
            timestamp = Time.time,
            objPos = trackedObject.transform.position,
            objRot = trackedObject.transform.rotation,
            camPos = playspaceTransform.position,
            camRot = playspaceTransform.rotation,
            initialDistance = initialDistance,
            currentDistance = Vector3.Distance(trackedObject.transform.position, playspaceTransform.position),
            timeSign = timeSign
        };

        entry.changeInDistance = Mathf.Abs(entry.initialDistance - entry.currentDistance);

        if (refObject != null)
        {
            entry.refPos = refObject.transform.position;
            entry.refRot = refObject.transform.rotation;
            entry.refToTrackedDistance = Vector3.Distance(entry.refPos, entry.objPos);
            entry.rotationDifference = Quaternion.Angle(entry.refRot, entry.objRot);
        }
        else
        {
            // Fallback if reference object is not assigned
            entry.refPos = Vector3.zero;
            entry.refRot = Quaternion.identity;
            entry.refToTrackedDistance = -1f;
            entry.rotationDifference = -1f;
        }

        logQueue.Enqueue(entry);
    }

    /// Thread function that writes queued log entries to CSV asynchronously.
    private void WriteLogThread()
    {
        while (isLoggingThreadRunning)
        {
            while (logQueue.TryDequeue(out LogDataEntry entry))
            {
                string[] values = new string[]
                {
                    entry.timestamp.ToString("F3"),
                    entry.objPos.x.ToString("F3"), entry.objPos.y.ToString("F3"), entry.objPos.z.ToString("F3"),
                    entry.objRot.x.ToString("F3"), entry.objRot.y.ToString("F3"), entry.objRot.z.ToString("F3"), entry.objRot.w.ToString("F3"),
                    entry.refPos.x.ToString("F3"), entry.refPos.y.ToString("F3"), entry.refPos.z.ToString("F3"),
                    entry.refRot.x.ToString("F3"), entry.refRot.y.ToString("F3"), entry.refRot.z.ToString("F3"), entry.refRot.w.ToString("F3"),
                    entry.camPos.x.ToString("F3"), entry.camPos.y.ToString("F3"), entry.camPos.z.ToString("F3"),
                    entry.camRot.x.ToString("F3"), entry.camRot.y.ToString("F3"), entry.camRot.z.ToString("F3"), entry.camRot.w.ToString("F3"),
                    entry.initialDistance.ToString("F3"), entry.currentDistance.ToString("F3"), entry.changeInDistance.ToString("F3"),
                    entry.refToTrackedDistance.ToString("F3"), entry.rotationDifference.ToString("F3"), entry.timeSign.ToString("F3")
                };

                string line = string.Join(";", values);

                try
                {
                    writer?.WriteLine(line);
                    writer?.Flush(); // Write immediately to disk
                }
                catch (Exception ex)
                {
                    Debug.LogError("Logger write error: " + ex.Message);
                }
            }

            Thread.Sleep(logThreadSleepMs); // Reduce CPU usage
        }
    }
    #endregion

    #region Getters & Setters

    /// Set the external time sign for synchronization purposes.
    public void SetTimeSign(int value)
    {
        timeSign = value;
        Debug.Log("Logger timeSign updated externally: " + timeSign);
    }

    /// Get the current time sign.
    public int GetTimeSign()
    {
        return timeSign;
    }
    #endregion


}
