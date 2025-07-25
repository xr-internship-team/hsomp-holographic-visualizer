using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public GameObject trackedObject;        // Takip edilen obje (örneğin küp)
    public Transform playspaceTransform;    // MixedRealityPlayspace'in Transform'u

    private string filePath;
    private StreamWriter writer;

    private float logInterval = 0.1f;
    private float timeSinceLastLog = 0f;
    private bool loggingStarted = false;
    private float initialDistance = 0f;

    void Start()
    {
        try
        {
            filePath = Path.Combine(Application.temporaryCachePath, "DistanceLog.csv");
            writer = new StreamWriter(filePath, false);
            writer.WriteLine("Time,ObjectX,ObjectY,ObjectZ,CameraX,CameraY,CameraZ,InitialDistance,CurrentDistance,ChangeInDistance");

            if (trackedObject != null && playspaceTransform != null)
            {
                Vector3 objPos = trackedObject.transform.position;
                Vector3 camPos = playspaceTransform.position;
                initialDistance = Vector3.Distance(objPos, camPos);
            }
            else
            {
                Debug.LogWarning("Logger: trackedObject veya playspaceTransform atanmamış.");
            }

            loggingStarted = true;
            Debug.Log("Logging started: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Logger başlatılamadı: " + ex.Message);
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
        if (trackedObject == null || playspaceTransform == null)
        {
            Debug.LogWarning("LogData: trackedObject veya playspaceTransform null.");
            return;
        }

        Vector3 objPos = trackedObject.transform.position;
        Vector3 camPos = playspaceTransform.position;

        float currentDistance = Vector3.Distance(objPos, camPos);
        float changeInDistance = Mathf.Abs(initialDistance - currentDistance);
        float time = Time.time;

        string line = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
            time.ToString("F3"),
            objPos.x.ToString("F3"), objPos.y.ToString("F3"), objPos.z.ToString("F3"),
            camPos.x.ToString("F3"), camPos.y.ToString("F3"), camPos.z.ToString("F3"),
            initialDistance.ToString("F3"), currentDistance.ToString("F3"), changeInDistance.ToString("F3"));

        try
        {
            writer.WriteLine(line);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Logger yazma hatası: " + ex.Message);
        }
    }

    private void OnApplicationQuit()
    {
        try
        {
            if (writer != null)
            {
                writer.Close();
                Debug.Log("Kayıt durduruldu ve dosya kapatıldı.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Dosya kapatılırken hata oluştu: " + ex.Message);
        }
    }
}
