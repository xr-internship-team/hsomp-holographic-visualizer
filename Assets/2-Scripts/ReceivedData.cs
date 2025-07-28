using System;

[Serializable] 
public class ReceivedData
{
    public string timestamp;
    public int id;
    public float[] translation;
    public float[] quaternion;
}
