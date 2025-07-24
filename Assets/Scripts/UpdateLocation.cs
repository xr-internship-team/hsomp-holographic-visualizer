using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLocation : MonoBehaviour
{
    [SerializeField] QuaternionMatrix quaternionMatrix; // QuaternionMatrix referansı
    [SerializeField] InitializePoint initializePoint; // InitializePoint referansı
    [SerializeField] MarkerLocation markerLocation; // MarkerLocation referansı

    public GameObject worldPoint; // Dünya'da gösterilecek nokta

    public void UpdateLocationOfTarget()
    {
        // QuaternionMatrix'ten matrisi al
        Matrix4x4 worldToLocal = quaternionMatrix.WorldToLocalMatrix();

        // Başlangıç noktasını al
        Vector3 initialPoint = initializePoint.InitialPoint;

        // Dünya koordinat sisteminden yerel koordinat sistemine dönüşüm
        Vector3 localPoint = worldToLocal.MultiplyPoint3x4(initialPoint);

        Vector3 transformedWorld = quaternionMatrix.LocalToWorldMatrix().MultiplyVector(localPoint)
                                     + markerLocation.MarkerPosition();

        // worldPoint objesini bu pozisyona taşı               
        worldPoint.transform.position = transformedWorld;

        // Şimdi tersini yapalım
        Vector3 recoveredLocal = worldToLocal.MultiplyVector(transformedWorld - markerLocation.MarkerPosition());

        Debug.Log("Local Point: " + localPoint);
        Debug.Log("Transformed World Point: " + transformedWorld);
        Debug.Log("Recovered Local Point: " + recoveredLocal);
    }
}
