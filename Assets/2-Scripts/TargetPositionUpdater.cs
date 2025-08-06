using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    public Transform markerTransform;
    public Transform objectTransform;

    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    private float smoothingSpeed = 5f; // yumuþaklýk seviyesi (arttýrýrsan daha hýzlý geçer)

    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif)
    {
        var originalQuaternion = rotationDif;
        var invertedQuaternion = new Quaternion(
            -originalQuaternion.x,
            -originalQuaternion.y,
            -originalQuaternion.z,
            originalQuaternion.w
        );

        var originalVector = positionDif;
        var invertedVector = new Vector3(
            -originalVector.x,
            -originalVector.y,
            -originalVector.z);

        _targetPosition = markerTransform.position - objectTransform.rotation * invertedVector;
        _targetRotation = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion);
    }

    private void Update()
    {
        if (objectTransform == null) return;

        objectTransform.position = Vector3.Lerp(
            objectTransform.position,
            _targetPosition,
            Time.deltaTime * smoothingSpeed
        );

        objectTransform.rotation = Quaternion.Slerp(
            objectTransform.rotation,
            _targetRotation,
            Time.deltaTime * smoothingSpeed
        );
    }

    public void SetSmoothingSpeed(float value)
    {
        smoothingSpeed = value;
        Debug.Log("Smooth Level updated: " + smoothingSpeed);
    }
    public float GetSmoothingSpeed()
    {
        return smoothingSpeed;
    }
}
