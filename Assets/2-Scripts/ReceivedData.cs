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

    // New helper property to convert the Unix timestamp (double) to a DateTime object.
    // This is needed for the interpolation logic.
    public DateTime TimestampAsDateTime => DateTime.UnixEpoch.AddSeconds(timestamp);

    public double GetTimeStamp()
    {
        return timestamp;
    }

    public Vector3 GetPosition()
    {
        return new Vector3(
            positionDif[0],
            positionDif[1],
            positionDif[2]
        );
    }

    public Quaternion GetRotation()
    {
        return new Quaternion(
            rotationDif[0],
            rotationDif[1],
            rotationDif[2],
            rotationDif[3]
        );
    }
}