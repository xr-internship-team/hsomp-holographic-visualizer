using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public GameObject trackedObject;
    public Transform playspaceTransform;
    public GameObject refObject;

    private string filePath;
    private StreamWriter writer;

    private float logInterval = 0.1f;
    private float timeSinceLastLog = 0f;
    private bool loggingStarted = false;
    private float initialDistance = 0f;
    private int timeSign = 0;

    private Thread logThread;
    private bool isLoggingThreadRunning = true;

    private ConcurrentQueue<LogDataEntry> logQueue = new ConcurrentQueue<LogDataEntry>();

    // Veri yapısı
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
    void Start()
    {
        try
        {
            filePath = Path.Combine(Application.persistentDataPath, "DistanceLog.csv");
            Debug.Log("Logger path: " + filePath);
            writer = new StreamWriter(filePath, false);
            writer.WriteLine("Time;ObjectX;ObjectY;ObjectZ;ObjectRotX;ObjectRotY;ObjectRotZ;ObjectRotW;" +
                             "RefX;RefY;RefZ;RefRotX;RefRotY;RefRotZ;RefRotW;" +
                             "CameraX;CameraY;CameraZ;CameraRotX;CameraRotY;CameraRotZ;CameraRotW;" +
                             "InitialDistance;CurrentDistance;ChangeInDistance;RefToTrackedObjDistance;RefToTrackedObjRotationDiff;TimeSign");

            if (trackedObject != null && playspaceTransform != null)
            {
                initialDistance = Vector3.Distance(trackedObject.transform.position, playspaceTransform.position);
            }

            loggingStarted = true;
            logThread = new Thread(WriteLogThread);
            logThread.Start();

            Debug.Log("Logger started.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Logger init error: " + ex.Message);
        }
    }

    void Update()
    {
        if (!loggingStarted) return;

        timeSinceLastLog += Time.deltaTime;

        if (timeSinceLastLog >= logInterval)
        {
            EnqueueLogData();
            timeSinceLastLog = 0f;
        }
    }

    private void OnApplicationQuit()
    {
        isLoggingThreadRunning = false;
        logThread?.Join(); // Thread'in bitmesini bekle

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

    void EnqueueLogData()
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
            entry.refPos = Vector3.zero;
            entry.refRot = Quaternion.identity;
            entry.refToTrackedDistance = -1f;
            entry.rotationDifference = -1f;
        }

        logQueue.Enqueue(entry);
    }

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
                    writer?.Flush(); // Anında diske yaz, buffer büyümesin
                }
                catch (Exception ex)
                {
                    Debug.LogError("Logger write error: " + ex.Message);
                }
            }

            Thread.Sleep(10); // CPU tasarrufu için
        }
    }
    #endregion

    #region GetterSetter

    public void SetTimeSign(int value)
    {
        timeSign = value;
        Debug.Log("Logger timeSign updated externally: " + timeSign);
    }

    public int GetTimeSign()
    {
        return timeSign;
    }
    #endregion


}
