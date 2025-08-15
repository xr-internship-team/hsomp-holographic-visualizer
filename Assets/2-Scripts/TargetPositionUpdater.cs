using System.Collections;
using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    [Header("Transforms")]
    public Transform markerTransform;    // Marker object (e.g., main camera)
    public Transform objectTransform;    // Target object to be updated
    public Transform referenceCube;      // Manually placed reference cube in the scene

    [Header("Offset Configuration")]
    [Tooltip("Delay between multiple offset configurations in seconds.")]
    public float configureInterval = 1f;
    [Tooltip("Number of times to repeat offset configuration.")]
    public int configureSteps = 5;

    [Header("Smoothing")]
    [Tooltip("Higher values result in faster movement towards the target.")]
    public float smoothingSpeed = 1f;
    [Tooltip("Temporary smoothing speed applied when configuring offset.")]
    public float temporarySmoothingSpeed = 1.5f; // default value
    
    // Target position & rotation calculated from received data
    private Vector3 _targetPosition;
    private Quaternion _targetRotation;

    // Offsets to align target object with reference cube
    private Vector3 _positionOffset = Vector3.zero;
    private Quaternion _rotationOffset = Quaternion.identity;
    
    private bool _offsetConfigured = false;
    private bool _interpolationEnabled = true;
    

    /// Updates the target position and rotation based on received position and rotation differences.
    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif)
    {
        var invertedVector = -positionDif;
        var invertedQuaternion = new Quaternion(
            -rotationDif.x,
            -rotationDif.y,
            -rotationDif.z,
            rotationDif.w
        );

        // Calculate base rotation and position (g�zl��e g�re harici kamera nerede ve rotasyonu ne onu hesapl�yoruz)
        var baseRotation = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion);
        var basePosition = markerTransform.position - baseRotation * invertedVector;

        // Apply offsets if already configured
        if (_offsetConfigured)
        {
            basePosition += _positionOffset;
            baseRotation *= _rotationOffset;
        }

        _targetPosition = basePosition;
        _targetRotation = baseRotation;
    }

    /// Starts offset configuration multiple times with a temporary smoothing speed boost.
    public void ConfigureOffsetMultiple()
    {
        StartCoroutine(ConfigureOffsetRoutine());
        StartCoroutine(TemporaryBoostSmoothing(configureInterval*configureSteps, temporarySmoothingSpeed)); // Boost smoothing speed temporarily
    }

    /// Temporarily increases smoothing speed and gradually returns it to the original speed.
    private IEnumerator TemporaryBoostSmoothing(float duration, float boostedSpeed)
    {
        var originalSpeed = smoothingSpeed;
        smoothingSpeed = boostedSpeed;

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            smoothingSpeed = Mathf.Lerp(boostedSpeed, originalSpeed, elapsed / duration);
            yield return null;
        }

        smoothingSpeed = originalSpeed;
    }

    /// Runs ConfigureOffset() multiple times with a delay between each.
    private IEnumerator ConfigureOffsetRoutine()
    {
        for (var i = 0; i < configureSteps; i++)
        {
            ConfigureOffset();
            yield return new WaitForSeconds(configureInterval);
        }
    }

    /// Calculates and stores position and rotation offsets between the target object and the reference cube.
    private void ConfigureOffset()
    {
        _offsetConfigured = false;

        var objectPos = _targetPosition;
        var objectRot = _targetRotation;

        var referencePos = referenceCube.position;
        var referenceRot = referenceCube.rotation;

        var positionOffsetNew = referencePos - objectPos;
        var rotationOffsetNew = Quaternion.Inverse(objectRot) * referenceRot;
        rotationOffsetNew.Normalize();

        _positionOffset += positionOffsetNew;
        _rotationOffset *= rotationOffsetNew;

        _offsetConfigured = true;

        Debug.Log($"Offset set. Position offset: {_positionOffset}, Rotation offset: {_rotationOffset.eulerAngles}");
    }


    #region UnityEventFunctions
    private void Update()
    {
        if (objectTransform == null) return;

        if (_interpolationEnabled)
        {
            // Smoothly interpolate towards the target position and rotation
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
            // Instantly set position and rotation
            objectTransform.position = _targetPosition;
            objectTransform.rotation = _targetRotation;
        }
    }
    #endregion

    #region Getters & Setters
    public void SetSmoothingSpeed(float value) => smoothingSpeed = value;
    public float GetSmoothingSpeed() => smoothingSpeed;
    public void EnableInterpolation(bool enabled) => _interpolationEnabled = enabled;
    public bool IsInterpolationEnabled() => _interpolationEnabled;
    #endregion

}