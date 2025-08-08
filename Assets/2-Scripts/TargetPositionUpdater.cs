using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    public Transform markerTransform; // main camera
    public Transform objectTransform; // yeþil küp
    public Transform referenceCube; // Sahnedeki elle yerleþtirilen referans küp

    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    private Vector3 positionOffset = Vector3.zero;
    private Quaternion rotationOffset = Quaternion.identity;

    private float smoothingSpeed = 1f; // yumuþaklýk seviyesi (arttýrýrsan daha hýzlý geçer)

    private bool offsetConfigured = false;

    private bool interpolationEnabled = true;

    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif)
    {


        var invertedVector = -positionDif;
        var invertedQuaternion = new Quaternion(
            -rotationDif.x,
            -rotationDif.y,
            -rotationDif.z,
            rotationDif.w
        );

        Vector3 basePosition = markerTransform.position - objectTransform.rotation * invertedVector;
        Quaternion baseRotation = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion);

        if (offsetConfigured)
        {
            basePosition += positionOffset;
            baseRotation *= rotationOffset;
        }


        _targetPosition = basePosition;
        _targetRotation = baseRotation;
    }

    public void ConfigureOffset()
    {
        offsetConfigured = false;
        // Offset uygulanmamýþ hedef pozisyon/rotasyon üzerinden hesapla
        Vector3 objectPos = _targetPosition;
        Quaternion objectRot = _targetRotation;

        Vector3 referencePos = referenceCube.position;
        Quaternion referenceRot = referenceCube.rotation;

        var positionOffsetnew = referencePos - objectPos;
        var rotationOffsetnew = Quaternion.Inverse(objectRot) * referenceRot;
        positionOffset += positionOffsetnew;
        rotationOffset *= rotationOffsetnew;
        offsetConfigured = true;

        Debug.Log($"Offset set. Position offset: {positionOffset}, Rotation offset: {rotationOffset.eulerAngles}");
    }


    #region UnityEventFunctions
    private void Update()
    {
        if (objectTransform == null) return;

        if (interpolationEnabled)
        {
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
        else
        {
            // Direkt geçiþ (anlýk pozisyon ve rotasyon)
            objectTransform.position = _targetPosition;
            objectTransform.rotation = _targetRotation;
        }
    }
    #endregion

    #region GetterSetters
    public void SetSmoothingSpeed(float value)
    {
        smoothingSpeed = value;
    }
    public float GetSmoothingSpeed()
    {
        return smoothingSpeed;
    }
    public void EnableInterpolation(bool enabled)
    {
        interpolationEnabled = enabled;
    }

    public bool IsInterpolationEnabled()
    {
        return interpolationEnabled;
    }

    #endregion
}
