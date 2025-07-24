using UnityEngine;

public class DataReceiver : MonoBehaviour
{
    public GameObject outsideCamera;
    public GameObject marker;

    public TargetPositionUpdater targetPositionUpdater;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            var referencePosition = outsideCamera.transform.InverseTransformPoint(marker.transform.position);

            Debug.Log("Position: " + outsideCamera.transform.InverseTransformPoint(marker.transform.position));
            //Debug.Log("AngleY: " + Vector3.SignedAngle(outsideCamera.transform.forward, marker.transform.position - outsideCamera.transform.position, Vector3.up));

            Quaternion relativeRotation = Quaternion.Inverse(outsideCamera.transform.rotation) * marker.transform.rotation;

            // Euler olarak farkı al (X, Y, Z açılarında)
            Vector3 eulerDiff = relativeRotation.eulerAngles;

            // Açılar 0-360 aralığında döner; -180~180'e normalleştir
            eulerDiff = NormalizeEuler(eulerDiff);

            Debug.Log("Axis difference (B relative to A): " + eulerDiff);

            targetPositionUpdater.CubePositionSetter(referencePosition, eulerDiff);
        }
    }


    private Vector3 NormalizeEuler(Vector3 angles)
    {
        return new Vector3(
            NormalizeAngle(angles.x),
            NormalizeAngle(angles.y),
            NormalizeAngle(angles.z)
        );
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }
}
