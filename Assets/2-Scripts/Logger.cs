using System.IO;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class Logger : MonoBehaviour
{
    public GameObject trackedObject;        // Marker hesaplamasının uygulancağı küp
    public Transform playspaceTransform;    // MixedRealityPlayspace'in Transform'u
    public GameObject refObject;            // Manuel şekilde koyacağımız küp.
    public Interactable controlButton1;


    private string filePath;
    private StreamWriter writer;

    private float logInterval = 0.1f;
    private float timeSinceLastLog = 0f;
    private bool loggingStarted = false;
    private float initialDistance = 0f;

    private int timeSign = 0;

    void Start()
    {
        controlButton1.OnClick.AddListener(ButtonClicked);

        try
        {
            filePath = Path.Combine(Application.temporaryCachePath, "DistanceLog.csv");
            Debug.Log("file path: " + filePath);
            writer = new StreamWriter(filePath, false);
            writer.WriteLine("Time;ObjectX;ObjectY;ObjectZ;ObjectRotX;ObjectRotY;ObjectRotZ;ObjectRotW;" +
                "RefX;RefY;RefZ;RefRotX;RefRotY;RefRotZ;RefRotW;" +
                "CameraX;CameraY;CameraZ;CameraRotX;CameraRotY;CameraRotZ;CameraRotW;" +
                "InitialDistance;CurrentDistance;ChangeInDistance;RefToTrackedObjDistance;RefToTrackedObjRotationDiff;TimeSign");

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

    private void ButtonClicked()
    {
        timeSign += 1;
        Debug.Log("Button clicked. Time Sign: " + timeSign);
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
        Quaternion objRot = trackedObject.transform.rotation;

        Vector3 camPos = playspaceTransform.position;
        Quaternion camRot = playspaceTransform.rotation;

        Vector3 refPos = refObject != null ? refObject.transform.position : Vector3.zero;
        Quaternion refRot = refObject != null ? refObject.transform.rotation : Quaternion.identity;

        float refToTrackedDistance = -1f;
        if (refObject != null && trackedObject != null)
        {
            refToTrackedDistance = Vector3.Distance(refObject.transform.position, trackedObject.transform.position);
        }

        float rotationDifference = -1f;
        if (refObject != null && trackedObject != null)
        {
            rotationDifference = Quaternion.Angle(refRot, objRot);
        }


        float currentDistance = Vector3.Distance(objPos, camPos);
        float changeInDistance = Mathf.Abs(initialDistance - currentDistance);
        float time = Time.time;

        string[] values = new string[]
        {
        time.ToString("F3"),
        objPos.x.ToString("F3"), objPos.y.ToString("F3"), objPos.z.ToString("F3"),
        objRot.x.ToString("F3"), objRot.y.ToString("F3"), objRot.z.ToString("F3"), objRot.w.ToString("F3"),
        refPos.x.ToString("F3"), refPos.y.ToString("F3"), refPos.z.ToString("F3"),
        refRot.x.ToString("F3"), refRot.y.ToString("F3"), refRot.z.ToString("F3"), refRot.w.ToString("F3"),
        camPos.x.ToString("F3"), camPos.y.ToString("F3"), camPos.z.ToString("F3"),
        camRot.x.ToString("F3"), camRot.y.ToString("F3"), camRot.z.ToString("F3"), camRot.w.ToString("F3"),
        initialDistance.ToString("F3"), currentDistance.ToString("F3"), changeInDistance.ToString("F3"),
        refToTrackedDistance.ToString("F3"), rotationDifference.ToString("F3"), timeSign.ToString("F3")
        };
        string line = string.Join(";", values);


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