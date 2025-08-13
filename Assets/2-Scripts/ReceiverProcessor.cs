// ... usings

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ReceiverProcessor : MonoBehaviour
{
    public enum ProcessingMode { None, TimestampCompare, BufferInterpolation, Hybrid }

    [Header("Mode & Target")]
    public ProcessingMode currentMode = ProcessingMode.Hybrid;
    public TargetPositionUpdater targetPositionUpdater;

    [Header("UDP")]
    public int udpPort = 12345;

    [Header("Buffered/Hybrid Timing")]
    public double renderDelayMs = 30.0;
    public double maxBufferSeconds = 0.5;
    public double baseLeadMs = 18.0;
    public float minDeltaTime = 0.005f;

    private IReceiver _receiver;
    private Thread _thread;
    private volatile bool _running;

    private readonly List<ReceivedData> _buffer = new();
    private readonly object _lock = new();
    private double _timeSyncOffset = double.NaN;

    private static double UnityNow() => Time.realtimeSinceStartupAsDouble;
    private static double NormalizeUnix(double ts) => (ts > 1e12) ? ts / 1000.0 : ts;

    private void Start()
    {
        _receiver = new UdpReceiver(udpPort);
        _running = true;
        _thread = new Thread(ReceiveLoop) { IsBackground = true };
        _thread.Start();
    }

    private void Update()
    {
        switch (currentMode)
        {
            case ProcessingMode.None:
                if (TryPopLatest(out var d0))
                    targetPositionUpdater.CubePositionSetterTimed(d0.GetPosition(), d0.GetRotation(), UnityNow());
                break;

            case ProcessingMode.TimestampCompare:
                ApplyTimestampCompareTimed();
                break;

            case ProcessingMode.BufferInterpolation:
                if (TryGetInterpolatedState(out var p1, out var r1, out var remoteT1))
                {
                    double unityT = remoteT1 + _timeSyncOffset;
                    targetPositionUpdater.CubePositionSetterTimed(p1, r1, unityT);
                }
                else if (PeekLatest(out var d1))
                {
                    targetPositionUpdater.CubePositionSetterTimed(d1.GetPosition(), d1.GetRotation(), UnityNow());
                }
                break;

            case ProcessingMode.Hybrid:
                ApplyHybridTimed();
                break;
        }
    }

    // ---------- TimestampCompare (timed) ----------
    private void ApplyTimestampCompareTimed()
    {
        if (!TryPopLatest(out var nowData)) return;

        // Try peek previous for simple 2-sample interpolation
        if (!PeekPrev(out var prevData))
        {
            targetPositionUpdater.CubePositionSetterTimed(nowData.GetPosition(), nowData.GetRotation(), UnityNow());
            return;
        }

        double tPrev = NormalizeUnix(prevData.timestamp);
        double tNow  = NormalizeUnix(nowData.timestamp);
        double targetRemoteT = tNow - renderDelayMs / 1000.0;

        float alpha;
        if (Mathf.Abs((float)(tNow - tPrev)) < 1e-6f || targetRemoteT <= tPrev) alpha = 1f;
        else if (targetRemoteT >= tNow) alpha = 0f;
        else alpha = (float)((targetRemoteT - tPrev) / (tNow - tPrev));

        Vector3 pos = Vector3.Lerp(prevData.GetPosition(), nowData.GetPosition(), alpha);
        Quaternion rot = Quaternion.Slerp(prevData.GetRotation(), nowData.GetRotation(), alpha);

        double unitySampleT = targetRemoteT + _timeSyncOffset;
        targetPositionUpdater.CubePositionSetterTimed(pos, rot, unitySampleT);
    }

    // ---------- Hybrid (timed with short prediction) ----------
    private void ApplyHybridTimed()
    {
        if (!TryGetInterpolatedBracket(out var a, out var b, out var ta, out var tb,
                                       out var interpPos, out var interpRot, out var remoteRenderTime))
        {
            // fallbacks
            if (TryGetInterpolatedState(out var p, out var r, out var remT))
                targetPositionUpdater.CubePositionSetterTimed(p, r, remT + _timeSyncOffset);
            else if (PeekLatest(out var latest))
                targetPositionUpdater.CubePositionSetterTimed(latest.GetPosition(), latest.GetRotation(), UnityNow());
            return;
        }

        double dt = Mathf.Max(minDeltaTime, (float)(tb - ta));
        Vector3 vel = (b.GetPosition() - a.GetPosition()) / (float)dt;

        Quaternion dq = b.GetRotation() * Quaternion.Inverse(a.GetRotation());
        dq.ToAngleAxis(out float angleDeg, out Vector3 axis);
        if (float.IsNaN(axis.x) || axis == Vector3.zero) { axis = Vector3.up; angleDeg = 0f; }
        float angVelDegPerSec = angleDeg / (float)dt;

        double leadSec = Mathf.Max(0f, (float)baseLeadMs) / 1000.0;
        Vector3 predictedPos = interpPos + vel * (float)leadSec;
        Quaternion predictedRot = Quaternion.AngleAxis(angVelDegPerSec * (float)leadSec, axis.normalized) * interpRot;
        predictedRot.Normalize();

        double remoteTargetT = remoteRenderTime + leadSec;
        double unitySampleT  = remoteTargetT + _timeSyncOffset;

        targetPositionUpdater.CubePositionSetterTimed(predictedPos, predictedRot, unitySampleT);
    }

    // ---------- Interpolation helpers ----------
    private bool TryGetInterpolatedState(out Vector3 pos, out Quaternion rot, out double remoteT)
    {
        pos = default; rot = default; remoteT = 0;

        lock (_lock)
        {
            if (_buffer.Count < 2) return false;

            if (double.IsNaN(_timeSyncOffset))
                _timeSyncOffset = UnityNow() - NormalizeUnix(_buffer[0].timestamp);

            double unityRenderNow   = UnityNow() - (renderDelayMs / 1000.0);
            double remoteRenderTime = unityRenderNow - _timeSyncOffset;

            // slide window
            while (_buffer.Count >= 2)
            {
                double t0 = NormalizeUnix(_buffer[0].timestamp);
                double t1 = NormalizeUnix(_buffer[1].timestamp);
                if (t1 <= remoteRenderTime) _buffer.RemoveAt(0); else break;
            }
            if (_buffer.Count < 2) return false;

            var A = _buffer[0]; var B = _buffer[1];
            double ta = NormalizeUnix(A.timestamp), tb = NormalizeUnix(B.timestamp);

            if (remoteRenderTime <= ta) { pos = A.GetPosition(); rot = A.GetRotation(); remoteT = ta; return true; }
            if (remoteRenderTime >= tb) { pos = B.GetPosition(); rot = B.GetRotation(); remoteT = tb; return true; }

            float t = (float)((remoteRenderTime - ta) / Mathf.Max(1e-6f, (float)(tb - ta)));
            pos = Vector3.Lerp(A.GetPosition(), B.GetPosition(), t);
            rot = Quaternion.Slerp(A.GetRotation(), B.GetRotation(), t);
            remoteT = remoteRenderTime;
            return true;
        }
    }

    private bool TryGetInterpolatedBracket(out ReceivedData a, out ReceivedData b,
                                           out double ta, out double tb,
                                           out Vector3 interpPos, out Quaternion interpRot,
                                           out double remoteRenderTime)
    {
        a=null;b=null;ta=tb=0;interpPos=default;interpRot=default;remoteRenderTime=0;

        lock (_lock)
        {
            if (_buffer.Count < 2) return false;

            if (double.IsNaN(_timeSyncOffset))
                _timeSyncOffset = UnityNow() - NormalizeUnix(_buffer[0].timestamp);

            double unityRenderNow   = UnityNow() - (renderDelayMs / 1000.0);
            remoteRenderTime        = unityRenderNow - _timeSyncOffset;

            // slide window
            while (_buffer.Count >= 2)
            {
                double t0 = NormalizeUnix(_buffer[0].timestamp);
                double t1 = NormalizeUnix(_buffer[1].timestamp);
                if (t1 <= remoteRenderTime) _buffer.RemoveAt(0); else break;
            }
            if (_buffer.Count < 2) return false;

            a = _buffer[0]; b = _buffer[1];
            ta = NormalizeUnix(a.timestamp); tb = NormalizeUnix(b.timestamp);

            if (remoteRenderTime <= ta) { interpPos = a.GetPosition(); interpRot = a.GetRotation(); return true; }
            if (remoteRenderTime >= tb) { interpPos = b.GetPosition(); interpRot = b.GetRotation(); return true; }

            float t = (float)((remoteRenderTime - ta) / Mathf.Max(1e-6f, (float)(tb - ta)));
            interpPos = Vector3.Lerp(a.GetPosition(), b.GetPosition(), t);
            interpRot = Quaternion.Slerp(a.GetRotation(), b.GetRotation(), t);
            return true;
        }
    }

    // ---------- queue / receive ----------
    private void ReceiveLoop()
    {
        while (_running)
        {
            var d = _receiver.GetData();
            if (d == null) continue;

            double ts = NormalizeUnix(d.timestamp);
            lock (_lock)
            {
                _buffer.Add(d);
                PruneBuffer(ts);
            }
        }
    }

    private void PruneBuffer(double newestTs)
    {
        double minTs = newestTs - Mathf.Max(0.05f, (float)maxBufferSeconds);
        int remove = 0;
        for (int i = 0; i < _buffer.Count; i++)
        {
            double t = NormalizeUnix(_buffer[i].timestamp);
            if (t < minTs) remove++;
            else break;
        }
        if (remove > 0) _buffer.RemoveRange(0, Mathf.Min(remove, _buffer.Count));
        if (_buffer.Count > 256) _buffer.RemoveRange(0, _buffer.Count - 256);
    }

    private bool TryPopLatest(out ReceivedData d)
    {
        lock (_lock)
        {
            if (_buffer.Count == 0) { d=null; return false; }
            d = _buffer[^1];
            _buffer.Clear();
            return true;
        }
    }

    private bool PeekLatest(out ReceivedData d)
    {
        lock (_lock)
        {
            if (_buffer.Count == 0) { d=null; return false; }
            d = _buffer[^1];
            return true;
        }
    }
    private bool PeekPrev(out ReceivedData d)
    {
        lock (_lock)
        {
            if (_buffer.Count < 2) { d=null; return false; }
            d = _buffer[^2];
            return true;
        }
    }
    
    public void SetProcessingMode(int modeIndex)
    {
        int modeCount = Enum.GetValues(typeof(ProcessingMode)).Length;
        currentMode = (ProcessingMode)Mathf.Clamp(modeIndex, 0, modeCount - 1);
        Debug.Log("Processing mode changed to: " + currentMode);
    }

    private void OnDisable() { _running=false; _receiver?.Close(); if (_thread!=null && _thread.IsAlive) _thread.Join(); }
    private void OnDestroy() { _running=false; _receiver?.Close(); if (_thread!=null && _thread.IsAlive) _thread.Join(); }

    
}
