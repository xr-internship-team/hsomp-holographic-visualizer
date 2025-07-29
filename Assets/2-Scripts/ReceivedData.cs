using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] 
public class ReceivedData
{
    public string timestamp;
    public int id;
    public List<float> positionDif;
    public List<float> rotationDif;

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
