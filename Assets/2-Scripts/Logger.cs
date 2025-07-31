using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public Transform objectTransform;            // Object in Unity scene
    public Transform markerTransform;            // Marker (Main Camera) in Unity scene
    public Transform testObjectTransform;        // Test object

    private float distanceObjectToMarker;
    private Vector3 markerExternalPosition;       // Marker position w.r.t external camera (from Python)
    private Quaternion markerExternalRotation;    // Marker rotation w.r.t external camera (from Python)
    private float markerToExternalCameraDistance; // Distance from marker to external camera (from Python)

    private string filePath;
    string folderPath;
    

    private StreamWriter writer;
    private float logInterval = 0.1f;
    private float timeSinceLastLog = 0f;
    private bool loggingStarted = false;

    void Awake()
    {
        folderPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "VRLoggerLogs");
        Directory.CreateDirectory(folderPath);
        filePath = Path.Combine(folderPath, "VR_Object_Log.csv");

    }

    void Start()
    {
        try
        {
            writer = new StreamWriter(filePath, false);
            writer.WriteLine("Time,ObjectPosX,ObjectPosY,ObjectPosZ,ObjectRotX,ObjectRotY,ObjectRotZ,ObjectRotW," +
                             "MarkerPosX,MarkerPosY,MarkerPosZ,MarkerRotX,MarkerRotY,MarkerRotZ,MarkerRotW," +
                             "Distance_Object_Marker," + 
                             "TestObjectRotX,TestObjectRotY,TestObjectRotZ,TestObjectRotW," +
                             "MarkerExternalPosX,MarkerExternalPosY,MarkerExternalPosZ,MarkerExternalRotX,MarkerExternalRotY,MarkerExternalRotZ,MarkerExternalRotW," +
                             "Distance_Marker_ExternalCamera");

            loggingStarted = true;
            Debug.Log("Logger started: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Logger start failed: " + ex.Message);
        }
    }

    void Update()
    {
        if (!loggingStarted) return;

        timeSinceLastLog += Time.deltaTime;
        if (timeSinceLastLog >= logInterval)
        {
            LogData();
            timeSinceLastLog = 0f;
        }
    }

    void LogData()
    {
        if (objectTransform == null || markerTransform == null || testObjectTransform == null)
        {
            Debug.LogWarning("Logger: Missing transform reference.");
            return;
        }

        float time = Time.time;

        Vector3 objectPos = objectTransform.position;
        Quaternion objectRot = objectTransform.rotation;

        Vector3 markerPos = markerTransform.position;
        Quaternion markerRot = markerTransform.rotation;

        distanceObjectToMarker = Vector3.Distance(objectPos, markerPos);

        Vector3 markerExtPos = markerExternalPosition;
        Quaternion markerExtRot = markerExternalRotation;

        Quaternion testRot = testObjectTransform.rotation;

        string line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7}," +
                                     "{8},{9},{10},{11},{12},{13},{14}," +
                                     "{15}," +
                                     "{16},{17},{18},{19}," +
                                     "{20},{21},{22},{23},{24},{25},{26}," +
                                     "{27}",
                                     time.ToString("F3"),
                                     objectPos.x.ToString("F3"), objectPos.y.ToString("F3"), objectPos.z.ToString("F3"),
                                     objectRot.x.ToString("F3"), objectRot.y.ToString("F3"), objectRot.z.ToString("F3"), objectRot.w.ToString("F3"),
                                     markerPos.x.ToString("F3"), markerPos.y.ToString("F3"), markerPos.z.ToString("F3"),
                                     markerRot.x.ToString("F3"), markerRot.y.ToString("F3"), markerRot.z.ToString("F3"), markerRot.w.ToString("F3"),
                                     distanceObjectToMarker.ToString("F3"),
                                     testRot.x.ToString("F3"), testRot.y.ToString("F3"), testRot.z.ToString("F3"), testRot.w.ToString("F3"),
                                     markerExtPos.x.ToString("F3"), markerExtPos.y.ToString("F3"), markerExtPos.z.ToString("F3"),
                                     markerExtRot.x.ToString("F3"), markerExtRot.y.ToString("F3"), markerExtRot.z.ToString("F3"), markerExtRot.w.ToString("F3"),
                                     markerToExternalCameraDistance.ToString("F3"));

        try
        {
            writer.WriteLine(line);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Logger write error: " + ex.Message);
        }
    }
    
    public void SetExternalTrackingData(Vector3 position, Quaternion rotation)
    {
        markerExternalPosition = position;
        markerExternalRotation = rotation;
        markerToExternalCameraDistance = position.magnitude;
    }

    void OnApplicationQuit()
    {
        try
        {
            writer?.Close();
            Debug.Log("Logger finished and file saved.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Logger closing error: " + ex.Message);
        }
    }
}
