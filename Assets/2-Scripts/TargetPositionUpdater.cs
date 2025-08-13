using System.Collections.Generic;
using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    [Header("Transforms")]
    [Tooltip("HMD/Camera transform (MixedRealityPlayspace/Main Camera)")]
    public Transform markerTransform;   // HMD
    public Transform objectTransform;   // Main cube

    // Target pose coming from network (world space, already offset-applied upstream)
    private Vector3 _targetPosWorld;
    private Quaternion _targetRotWorld;
    private double _sampleUnityTime; // WHEN this target pose corresponds to (Unity timeline)

    // Smoothed pose actually applied to the object
    private Vector3 _smoothedPosWorld;
    private Quaternion _smoothedRotWorld;
    private bool _firstApplied = true;

    // ---------- Alignment (optional) ----------
    private Vector3 _alignmentOffset = Vector3.zero;
    private Quaternion _rotationOffset = Quaternion.identity;
    private bool _useAlignmentOffset = false;

    // ---------- Smoothing / Hard-lock ----------
    [Header("Smoothing")]
    public bool useExponentialSmoothing = true;
    [Tooltip("Normal follow sharpness (higher=faster)")]
    public float normalFollowSharpness = 10f;
    [Tooltip("Fast follow sharpness when error is large")]
    public float fastFollowSharpness = 25f;

    [Range(0f,1f)]
    [Tooltip("Legacy per-frame lerp (used if exponential is OFF)")]
    public float smoothFactor = 0.5f;

    [Header("Hard-Lock (anti-lag)")]
    public bool hardLockWhenFar = true;
    [Tooltip("If distance error exceeds this (m), use fast follow")]
    public float snapDistance = 0.02f;
    [Tooltip("If angular error exceeds this (deg), use fast follow")]
    public float snapAngleDeg = 1.5f;

    // ---------- Head pose history (for HCR) ----------
    private struct HeadPoseSample
    {
        public double t;
        public Vector3 p;
        public Quaternion r;
    }
    [Header("Head Compensation")]
    [Tooltip("How many seconds of HMD history to keep")]
    public float headHistorySeconds = 0.6f;
    private readonly List<HeadPoseSample> _headHistory = new List<HeadPoseSample>(256);

    // ---------- Public API ----------
    /// <summary>
    /// New: set target with the remote sample's Unity-time (seconds).
    /// </summary>
    public void CubePositionSetterTimed(Vector3 positionWorld, Quaternion rotationWorld, double unitySampleTime)
    {
        // Optional alignment offsets (keep your usage if you rely on them)
        if (_useAlignmentOffset)
        {
            positionWorld += _alignmentOffset;
            rotationWorld = _rotationOffset * rotationWorld;
        }

        _targetPosWorld = positionWorld;
        _targetRotWorld = rotationWorld;
        _sampleUnityTime = unitySampleTime;
    }

    /// <summary>
    /// Backwards compatibility: treat sample time as 'now'.
    /// </summary>
    public void CubePositionSetter(Vector3 positionWorld, Quaternion rotationWorld)
    {
        CubePositionSetterTimed(positionWorld, rotationWorld, Time.realtimeSinceStartupAsDouble);
    }

    public void SetSmoothFactor(float value)
    {
        smoothFactor = Mathf.Clamp01(value);
        Debug.Log($"Smooth factor set to {smoothFactor:0.00}");
    }

    public void CalculateAlignmentOffsetTo(Vector3 targetWorldPos, Quaternion targetWorldRot)
    {
        Vector3 basisPos = objectTransform.position;
        Quaternion basisRot = objectTransform.rotation;

        _alignmentOffset = targetWorldPos - basisPos;
        _rotationOffset  = targetWorldRot * Quaternion.Inverse(basisRot);
        _rotationOffset.Normalize();

        _useAlignmentOffset = true;
        Debug.Log($"Alignment offset set. Pos={_alignmentOffset}, Rot(Euler)={_rotationOffset.eulerAngles}");
    }

    // ---------- Head pose sampling ----------
    private void SampleHeadPose()
    {
        var s = new HeadPoseSample
        {
            t = Time.realtimeSinceStartupAsDouble,
            p = markerTransform.position,
            r = markerTransform.rotation
        };
        _headHistory.Add(s);

        // prune old
        double minT = s.t - Mathf.Max(0.2f, headHistorySeconds);
        int remove = 0;
        for (int i = 0; i < _headHistory.Count; i++)
        {
            if (_headHistory[i].t < minT) remove++;
            else break;
        }
        if (remove > 0) _headHistory.RemoveRange(0, Mathf.Min(remove, _headHistory.Count));
        if (_headHistory.Count > 300) _headHistory.RemoveRange(0, _headHistory.Count - 300);
    }

    private bool TryGetHeadPoseAt(double tQuery, out Vector3 p, out Quaternion r)
    {
        p = default; r = default;
        if (_headHistory.Count == 0) return false;

        // clamp extremes
        if (tQuery <= _headHistory[0].t)
        {
            p = _headHistory[0].p; r = _headHistory[0].r; return true;
        }
        if (tQuery >= _headHistory[^1].t)
        {
            p = _headHistory[^1].p; r = _headHistory[^1].r; return true;
        }

        // find bracket
        for (int i = 0; i < _headHistory.Count - 1; i++)
        {
            var a = _headHistory[i];
            var b = _headHistory[i + 1];
            if (a.t <= tQuery && tQuery <= b.t)
            {
                float t = (float)((tQuery - a.t) / Mathf.Max(1e-6f, (float)(b.t - a.t)));
                p = Vector3.Lerp(a.p, b.p, t);
                r = Quaternion.Slerp(a.r, b.r, t);
                return true;
            }
        }
        return false;
    }

    // ---------- HCR (compensate target by head delta) ----------
    private void GetHeadCompensatedTarget(out Vector3 outPos, out Quaternion outRot)
    {
        // Head at sample time
        if (!TryGetHeadPoseAt(_sampleUnityTime, out var headPos_then, out var headRot_then))
        {
            // No history — fallback: no compensation
            outPos = _targetPosWorld;
            outRot = _targetRotWorld;
            return;
        }

        // Head now
        var headPos_now = markerTransform.position;
        var headRot_now = markerTransform.rotation;

        // Convert target world → head-local at sample time
        // p_rel = R_then^-1 * (p_w - t_then)
        var p_rel = Quaternion.Inverse(headRot_then) * (_targetPosWorld - headPos_then);
        var r_rel = Quaternion.Inverse(headRot_then) * _targetRotWorld;

        // Bring head-local pose to NOW world using current head
        outPos = headRot_now * p_rel + headPos_now;
        outRot = headRot_now * r_rel;
    }

    // ---------- Update/LateUpdate ----------
    private void Update()
    {
        // Always sample HMD in Update (gets latest tracking)
        if (markerTransform != null) SampleHeadPose();
    }

    private void LateUpdate()
    {
        if (objectTransform == null) return;

        // 1) Compute head-compensated target pose
        GetHeadCompensatedTarget(out var targetPosComp, out var targetRotComp);

        // 2) Smoothing (with optional hard-lock)
        if (_firstApplied)
        {
            _smoothedPosWorld = targetPosComp;
            _smoothedRotWorld = targetRotComp;
            _firstApplied = false;
        }
        else
        {
            float posErr = Vector3.Distance(_smoothedPosWorld, targetPosComp);
            float rotErr = Quaternion.Angle(_smoothedRotWorld, targetRotComp);
            bool fast = hardLockWhenFar && (posErr > snapDistance || rotErr > snapAngleDeg);

            if (useExponentialSmoothing)
            {
                float sharp = fast ? fastFollowSharpness : normalFollowSharpness;
                float a = 1f - Mathf.Exp(-Mathf.Max(0f, sharp) * Time.deltaTime);
                _smoothedPosWorld = Vector3.Lerp(_smoothedPosWorld, targetPosComp, a);
                _smoothedRotWorld = Quaternion.Slerp(_smoothedRotWorld, targetRotComp, a);
            }
            else
            {
                float a = Mathf.Clamp01(fast ? Mathf.Max(0.85f, smoothFactor) : smoothFactor);
                _smoothedPosWorld = Vector3.Lerp(_smoothedPosWorld, targetPosComp, a);
                _smoothedRotWorld = Quaternion.Slerp(_smoothedRotWorld, targetRotComp, a);
            }
        }

        // 3) Apply
        objectTransform.position = _smoothedPosWorld;
        objectTransform.rotation = _smoothedRotWorld;
    }
}
