using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaternionMatrix : MonoBehaviour
{
    public MarkerLocation markerLocation;

    public Matrix4x4 LocalToWorldMatrix()
    {
        Matrix4x4 localToWorld = Matrix4x4.Rotate(markerLocation.MarkerRotation());
        return localToWorld;
    }

    public Matrix4x4 WorldToLocalMatrix()
    {
        Matrix4x4 localToWorld = LocalToWorldMatrix();
        Matrix4x4 worldToLocal = localToWorld.transpose; // ortonormal varsayıyoruz
        return worldToLocal;
    }
}
