using UnityEngine;

public class TargetLocationUpdater : MonoBehaviour
{
    public Transform markerOrigin;   // marker referansı
    public Transform worldPoint;     // world'de gösterilecek nokta

    public void RotateLogger()
    {
        // marker referans sistemine göre tanımlı bir nokta (örneğin x=1 birim ileri)
        Vector3 localPoint = new Vector3(8, 0, 0);

        // marker'ın dünya pozisyonu
        Vector3 T = markerOrigin.position;

        // marker'ın rotasyon matrisi (Quaternion -> Matrix)
        Matrix4x4 localToWorld = Matrix4x4.Rotate(markerOrigin.rotation);
        print($"R: {localToWorld}");
        Matrix4x4 worldToLocal = localToWorld.transpose; // ortonormal varsayıyoruz

        // p_world = R * p_local + T
        Vector3 transformedWorld = localToWorld.MultiplyVector(localPoint) + T;

        // worldPoint objesini bu pozisyona taşı
        worldPoint.position = transformedWorld;

        // Şimdi tersini yapalım
        Vector3 recoveredLocal = worldToLocal.MultiplyVector(transformedWorld - T);

        print("Local Point: " + localPoint);
        print("Transformed World Point: " + transformedWorld);
        print("Recovered Local Point: " + recoveredLocal);
    } 
}
