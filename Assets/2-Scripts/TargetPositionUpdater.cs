using System.Collections;
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

    public float temporarySmoothingSpeed = 1.5f; // default value

    private float smoothingSpeed = 1f; // yumuþaklýk seviyesi (arttýrýrsan daha hýzlý geçer)

    private bool offsetConfigured = false;

    private bool interpolationEnabled = true;

    public float configureInterval = 1f; // saniye cinsinden bekleme süresi
    public int configureSteps = 5;       // kaç defa tekrar edecek

    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif, Vector3 cameraPosition, Quaternion cameraRotation)
    {


        var invertedVector = -positionDif;
        var invertedQuaternion = new Quaternion(
            -rotationDif.x,
            -rotationDif.y,
            -rotationDif.z,
            rotationDif.w
        );
        Quaternion baseRotation = cameraRotation * Quaternion.Inverse(invertedQuaternion);
        Vector3 basePosition = cameraPosition - baseRotation * invertedVector;


        if (offsetConfigured)
        {
            basePosition += positionOffset;
            baseRotation *= rotationOffset;
        }


        _targetPosition = basePosition;
        _targetRotation = baseRotation;
    }

    public void ConfigureOffsetMultiple()
    {
        StartCoroutine(ConfigureOffsetRoutine());
        StartCoroutine(TemporaryBoostSmoothing(configureInterval * configureSteps, temporarySmoothingSpeed)); // Boost smoothing speed temporarily

    }

    private IEnumerator TemporaryBoostSmoothing(float duration, float boostedSpeed)
    {
        float originalSpeed = smoothingSpeed;
        smoothingSpeed = boostedSpeed;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            smoothingSpeed = Mathf.Lerp(boostedSpeed, originalSpeed, elapsed / duration);
            yield return null;
        }

        smoothingSpeed = originalSpeed;
    }

    private IEnumerator ConfigureOffsetRoutine()
    {
        for (int i = 0; i < configureSteps; i++)
        {
            ConfigureOffset();
            yield return new WaitForSeconds(configureInterval);
        }
    }

    public void ConfigureOffset()
    {
        offsetConfigured = false;

        Vector3 objectPos = _targetPosition;
        Quaternion objectRot = _targetRotation;

        Vector3 referencePos = referenceCube.position;
        Quaternion referenceRot = referenceCube.rotation;

        var positionOffsetnew = referencePos - objectPos;
        var rotationOffsetnew = Quaternion.Inverse(objectRot) * referenceRot;
        rotationOffsetnew.Normalize();

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