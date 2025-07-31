using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLogger : MonoBehaviour
{
    public Transform targetObject;
    public Transform testObject;
    public Transform camera;
    
    private string filePath;
    private string folderPath;
    private StreamWriter writer;
    
    private float logInterval = 0.1f;
    private float timeSinceLastLog = 0f;
    private bool loggingStarted = false;
    
    void Awake()
    {
        folderPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "VR_Test_Logs");
        Directory.CreateDirectory(folderPath);
        filePath = Path.Combine(folderPath, "VR_Test_Log.csv");
    }
    void Start()
    {
        try
        {
            writer = new StreamWriter(filePath, false);
            writer.WriteLine("Time..."); // Düzenleme yapılacak

            loggingStarted = true;
            Debug.Log("Logger started: " + filePath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Logger start failed: " + ex.Message);
        }
    }
    }
    
    void Update()
    {
        if (!loggingStarted) return;

        timeSinceLastLog += Time.deltaTime;
        if (timeSinceLastLog >= logInterval)
        {
            TestData();
            timeSinceLastLog = 0f;
        }
    }

    void TestData()
    {
        // Düzenleme yapılacak
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
