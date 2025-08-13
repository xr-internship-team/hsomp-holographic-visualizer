using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    [Header("Scene References")]
    public Transform markerTransform;     // Outside-in reference (world frame provider)
    public Transform objectTransform;     // Main object (cube)

    [Header("Base Smoothing")]
    [Range(0f, 1f)] public float baseSmoothFactor = 0.7f;
    [Range(0f, 1f)] public float minSmooth = 0.35f;
    [Range(0f, 1f)] public float maxSmooth = 0.95f;

    [Header("Adaptive Smoothing Thresholds")]
    public float lowSpeedThreshold = 0.02f;     // m/s (below → very smooth)
    public float highSpeedThreshold = 0.15f;    // m/s (above → more responsive)
    public float lowRotSpeedDeg = 5f;           // deg/s
    public float highRotSpeedDeg = 30f;         // deg/s

    [Header("Jitter Deadzone")]
    public float positionDeadzone = 0.0005f;    // 0.5 mm
    public float rotationDeadzoneDeg = 0.05f;   // 0.05°

    [Header("Spike Guard (Soft Clamp)")]
    public float spikePosMeters = 0.08f;        // jumps above this are softened
    public float spikeRotDeg = 30f;             // large rotation jumps are softened
    [Range(0f, 1f)] public float spikeBlend = 0.15f; // how much to move toward big jump per frame

    // Internal state
    private Vector3 _smoothedPosition;
    private Quaternion _smoothedRotation;
    private bool _firstUpdate = true;

    // Alignment state
    private Vector3 _alignmentOffset = Vector3.zero;
    private Quaternion _rotationOffset = Quaternion.identity;
    private bool _useAlignmentOffset = false;

    // Keep latest RAW pose (idempotent alignment)
    private Vector3 _lastRawPosition;
    private Quaternion _lastRawRotation = Quaternion.identity;
    private bool _hasRawPose = false;

    // For adaptive smoothing
    private Vector3 _lastAppliedPosition;
    private Quaternion _lastAppliedRotation;
    private float _lastAppliedTime;

    // Confidence coming from Python (0..1), -1 means unknown
    private float _latestConfidence = -1f;

    public void UpdateConfidence(float confidence) // call before CubePositionSetter if available
    {
        _latestConfidence = confidence;
    }

    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif)
    {
        // Convert incoming diffs (Unity ← outside tracker)
        var invertedVector = new Vector3(-positionDif.x, -positionDif.y, -positionDif.z);
        var invertedQuaternion = new Quaternion(-rotationDif.x, -rotationDif.y, -rotationDif.z, rotationDif.w);

        // 1) RAW pose (no offsets)
        var rawRotation = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion);
        var rawPosition = markerTransform.position - rawRotation * invertedVector;

        // Save for idempotent alignment
        _lastRawPosition = rawPosition;
        _lastRawRotation = rawRotation;
        _hasRawPose = true;

        // 2) Alignment offsets (if enabled)
        Vector3 finalPosition = _useAlignmentOffset ? rawPosition + _alignmentOffset : rawPosition;
        Quaternion finalRotation = _useAlignmentOffset ? _rotationOffset * rawRotation : rawRotation;

        // 3) Deadzone: ignore tiny changes to kill micro jitter
        if (!_firstUpdate)
        {
            if ((finalPosition - _smoothedPosition).magnitude < positionDeadzone)
                finalPosition = _smoothedPosition;

            if (Quaternion.Angle(finalRotation, _smoothedRotation) < rotationDeadzoneDeg)
                finalRotation = _smoothedRotation;
        }

        // 4) Spike guard: soften very large instantaneous jumps
        if (!_firstUpdate)
        {
            float posJump = (finalPosition - _smoothedPosition).magnitude;
            if (posJump > spikePosMeters)
            {
                finalPosition = Vector3.Lerp(_smoothedPosition, finalPosition, spikeBlend);
            }

            float rotJump = Quaternion.Angle(_smoothedRotation, finalRotation);
            if (rotJump > spikeRotDeg)
            {
                finalRotation = Quaternion.Slerp(_smoothedRotation, finalRotation, spikeBlend);
            }
        }

        // 5) Adaptive smoothing based on motion speed and confidence
        float dt = Mathf.Max(Time.time - _lastAppliedTime, 1e-4f);
        float posSpeed = (_firstUpdate ? 0f : (finalPosition - _lastAppliedPosition).magnitude / dt);         // m/s
        float rotSpeedDeg = (_firstUpdate ? 0f : Quaternion.Angle(_lastAppliedRotation, finalRotation) / dt); // deg/s

        float speedT = Mathf.InverseLerp(lowSpeedThreshold, highSpeedThreshold, posSpeed);
        float rotT = Mathf.InverseLerp(lowRotSpeedDeg, highRotSpeedDeg, rotSpeedDeg);
        float motionT = Mathf.Clamp01(Mathf.Max(speedT, rotT)); // be responsive if either is fast

        // Base → responsive mapping: high motion → lower smoothing
        float adaptiveSmooth = Mathf.Lerp(maxSmooth, minSmooth, motionT);

        // Confidence weighting: low confidence → increase smoothing (be conservative)
        if (_latestConfidence >= 0f)
        {
            // map conf∈[0,1] to a multiplier in [1.15, 0.85] (low conf => stronger smoothing)
            float confMul = Mathf.Lerp(1.15f, 0.85f, Mathf.Clamp01(_latestConfidence));
            adaptiveSmooth = Mathf.Clamp01(adaptiveSmooth * confMul);
        }

        // Blend with baseSmoothFactor so UI knob still has effect
        float smoothFactor = Mathf.Clamp01(0.5f * adaptiveSmooth + 0.5f * baseSmoothFactor);

        // 6) Initialize or smooth
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

        // 7) Apply
        objectTransform.position = _smoothedPosition;
        objectTransform.rotation = _smoothedRotation;

        // 8) Bookkeeping for next frame
        _lastAppliedPosition = _smoothedPosition;
        _lastAppliedRotation = _smoothedRotation;
        _lastAppliedTime = Time.time;
        _latestConfidence = -1f; // consume once
    }

    public void SetSmoothFactor(float value) // called by SmoothFactorController
    {
        baseSmoothFactor = Mathf.Clamp01(value);
        Debug.Log($"[TargetPositionUpdater] Base smooth factor set to {baseSmoothFactor:0.00}");
    }

    // Compute alignment relative to RAW pose when available
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
            basisPos = _firstUpdate ? objectTransform.position : _smoothedPosition;
            basisRot = _firstUpdate ? objectTransform.rotation : _smoothedRotation;
        }

        _alignmentOffset = targetWorldPosition - basisPos;
        _rotationOffset = targetWorldRotation * Quaternion.Inverse(basisRot);
        _useAlignmentOffset = true;

        Debug.Log("[TargetPositionUpdater] Alignment offset calculated and enabled.");
    }
}
