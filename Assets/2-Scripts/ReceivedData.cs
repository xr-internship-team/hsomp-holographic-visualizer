using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ReceivedData
{
    public double timestamp;
    public int id;
    public List<float> positionDif;
    public List<float> rotationDif;

    // Optional (sent by Python when SEND_CONFIDENCE_TO_UNITY = True)
    public float confidence = -1f;        // 0..1, -1 means unknown/not provided
    public float decision_margin = -1f;   // raw detector metric (optional)

    public Vector3 GetPosition()
    {
        return new Vector3(positionDif[0], positionDif[1], positionDif[2]);
    }

    public Quaternion GetRotation()
    {
        return new Quaternion(rotationDif[0], rotationDif[1], rotationDif[2], rotationDif[3]);
    }

    public float GetConfidence()
    {
        return confidence;
    }
}