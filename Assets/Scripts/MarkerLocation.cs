using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerLocation : MonoBehaviour
{
    public GameObject marker;   // marker referansı
    
    public Vector3 MarkerPosition()
    {
        return marker.transform.position;
    }

    public Quaternion MarkerRotation()
    {
        return marker.transform.rotation;
    }

}
