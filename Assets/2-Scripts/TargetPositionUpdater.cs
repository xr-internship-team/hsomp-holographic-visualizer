using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    public Transform markerTransform;      // VR headset (world frame provider)
    public Transform objectTransform;      // Main object (cube)

    private Vector3 _smoothedPosition;
    private Quaternion _smoothedRotation;
    private bool _firstUpdate = true;
    public float smoothFactor;

    // Alignment state
    private Vector3 alignmentOffset = Vector3.zero;
    private Quaternion rotationOffset = Quaternion.identity;
    private bool useAlignmentOffset = false;

    // Keep the latest RAW pose (computed WITHOUT any offsets)
    private Vector3 _lastRawPosition;
    private Quaternion _lastRawRotation = Quaternion.identity;
    private bool _hasRawPose = false; // first-frame safety

    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif)
    {
        // Convert incoming diffs
        var invertedVector = new Vector3(-positionDif.x, -positionDif.y, -positionDif.z);
        var invertedQuaternion = new Quaternion(-rotationDif.x, -rotationDif.y, -rotationDif.z, rotationDif.w);

        // 1) RAW pose (no offsets)
        var rawRotation = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion);
        var rawPosition = markerTransform.position - rawRotation * invertedVector;

        // Save for idempotent alignment calculations
        _lastRawPosition = rawPosition;
        _lastRawRotation = rawRotation;
        _hasRawPose = true;

        // 2) Final pose (apply alignment offsets only if enabled)
        Vector3 finalPosition = useAlignmentOffset ? rawPosition + alignmentOffset : rawPosition;
        Quaternion finalRotation = useAlignmentOffset ? rotationOffset * rawRotation : rawRotation;

        // 3) Smoothing
        if (_firstUpdate)
        {
            _smoothedPosition = finalPosition;
            _smoothedRotation = finalRotation;
            _firstUpdate = false;
        }
        else
        {
            _smoothedPosition = Vector3.Lerp(_smoothedPosition, finalPosition, smoothFactor);
            _smoothedRotation = Quaternion.Slerp(_smoothedRotation, finalRotation, smoothFactor);
        }

        // 4) Apply
        objectTransform.position = _smoothedPosition;
        objectTransform.rotation = _smoothedRotation;
    }

    public void SetSmoothFactor(float value)
    {
        smoothFactor = value;
        Debug.Log($"Smooth factor set to {smoothFactor}");
    }

    // Calculate offsets relative to RAW pose when available; otherwise use current displayed (smoothed) pose
    public void CalculateAlignmentOffsetTo(Vector3 targetWorldPosition, Quaternion targetWorldRotation)
    {
        Vector3 basisPos;
        Quaternion basisRot;

        if (_hasRawPose)
        {
            basisPos = _lastRawPosition;
            basisRot = _lastRawRotation;
        }
        else
        {
            // First click before first UDP update → use current visual pose to avoid huge jump
            basisPos = _firstUpdate ? objectTransform.position : _smoothedPosition;
            basisRot = _firstUpdate ? objectTransform.rotation : _smoothedRotation;
        }

        alignmentOffset = targetWorldPosition - basisPos;
        rotationOffset = targetWorldRotation * Quaternion.Inverse(basisRot); 
        useAlignmentOffset = true;
        Debug.Log("TargetPositionUpdater: Alignment offset calculated (" + (_hasRawPose ? "RAW" : "SMOOTHED") + ") and enabled.");
    }

    public bool HasValidRawPose() => _hasRawPose;
    
}