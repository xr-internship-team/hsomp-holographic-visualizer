using System;
using System.Collections.Generic;
using UnityEngine;

/// Represents the data structure received over UDP.
/// Stores timestamp, ID, position difference, and rotation difference.
[Serializable] 
public class ReceivedData
{
    public double timestamp;           // Time when the data was sent (Unix Epoch timestamp)
    public int id;                      // Identifier for the sender/object
    public List<float> positionDif;     // Position difference values (x, y, z)
    public List<float> rotationDif;     // Rotation difference values (x, y, z, w)


    #region Getters & Setters
    /// Returns the timestamp of the received data.
    public double GetTimeStamp()
    {
        return timestamp;
    }

    /// Converts positionDif list to a Unity Vector3.
    public Vector3 GetPosition()
    {
        return new Vector3(
            positionDif[0],
            positionDif[1],
            positionDif[2]
        );
    }

    /// Converts rotationDif list to a Unity Quaternion.
    public Quaternion GetRotation()
    {
        return new Quaternion(
            rotationDif[0],
            rotationDif[1],
            rotationDif[2],
            rotationDif[3]
        );
    }
    #endregion
}
