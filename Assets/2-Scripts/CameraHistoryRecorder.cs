using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraHistoryRecorder : MonoBehaviour
{
    public int maxHistoryCount = 300; // Kaç frame saklanacak
    private List<CameraSnapshot> _history = new List<CameraSnapshot>();

    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_mainCamera == null) return;

        // Unix epoch formatýnda saniye cinsinden
        double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

        Vector3 pos = _mainCamera.transform.position;
        Quaternion rot = _mainCamera.transform.rotation;

        _history.Add(new CameraSnapshot(now, pos, rot));

        // Eski kayýtlarý sil
        if (_history.Count > maxHistoryCount)
        {
            _history.RemoveAt(0);
        }
    }

    /// <summary>
    /// Verilen zamana en yakýn snapshot'ý bulur.
    /// </summary>
    public CameraSnapshot? GetClosestSnapshot(double targetTimestamp)
    {
        if (_history.Count == 0) return null;

        CameraSnapshot closest = _history[0];
        double minDiff = Math.Abs(closest.timestamp - targetTimestamp);

        foreach (var snap in _history)
        {
            double diff = Math.Abs(snap.timestamp - targetTimestamp);
            if (diff < minDiff)
            {
                minDiff = diff;
                closest = snap;
            }
        }

        return closest;
    }
}

[System.Serializable]
public struct CameraSnapshot
{
    public double timestamp;   // Unix time (saniye)
    public Vector3 position;
    public Quaternion rotation;

    public CameraSnapshot(double ts, Vector3 pos, Quaternion rot)
    {
        timestamp = ts;
        position = pos;
        rotation = rot;
    }
}
