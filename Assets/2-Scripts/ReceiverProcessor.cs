using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ReceiverProcessor : MonoBehaviour
{
    public enum ProcessingMode
    {
        None = 0,
        BufferInterpolation = 1,
        TimestampCompare = 2,
        HybridBuffer = 3
    }

    [Header("Mode")]
    public ProcessingMode currentMode = ProcessingMode.BufferInterpolation;
    public TargetPositionUpdater targetPositionUpdater;

    private IReceiver _receiver;
    private Thread _receiveThread;
    private readonly Queue<ReceivedData> _receivedDataQueue = new(128);
    private double _lastTimestamp;

    // ===== Delay & Jitter estimation =====
    private double _predictedDelay = 0.03;           // seconds (smoothed)
    private const double DelaySmoothFactor = 0.10;   // EMA for delay

    // Jitter: EMA of absolute deviation of delay (ms)
    [Header("Auto Buffer Sizing")]
    [Tooltip("EMA smoothing for jitter (0..1)")]
    [Range(0f, 1f)] public float jitterEmaAlpha = 0.20f;
    [Tooltip("Extra headroom added to buffer (ms)")]
    public double bufferSafetyMs = 10.0;
    [Tooltip("Min buffer window (ms)")]
    public double minBufferMs = 40.0;
    [Tooltip("Max buffer window (ms)")]
    public double maxBufferMs = 200.0;
    [Tooltip("Scale factor on (delay+jitter) before safety/clamp")]
    public float bufferScale = 1.25f;

    private double _delayEmaMs;  // smoothed delay in ms (for jitter baseline)
    private double _jitterEmaMs; // smoothed jitter in ms

    // ===== BufferInterpolation =====
    private readonly List<ReceivedData> _buffer = new();
    // Window is dynamic; we still keep a hard cap on stored packets:
    private const int MaxBufferedPackets = 256;

    // ===== TimestampCompare =====
    private ReceivedData _lastData;

    // ===== Outlier guard (prevents huge single-frame jumps) =====
    [Header("Outlier Guard")]
    public float maxStepPosMeters = 0.12f;       // hard reject if Δpos > this
    public float maxStepRotDeg = 60f;            // hard reject if Δrot > this

    private void Start()
    {
        _receiver = new UdpReceiver(12345);
        RunThread();
    }

    private void Update()
    {
        ReceivedData data = null;

        lock (_receivedDataQueue)
        {
            if (_receivedDataQueue.Count > 0)
                data = _receivedDataQueue.Dequeue();
        }

        if (data == null) return;

        // Update delay/jitter stats
        EstimateCurrentDelayAndJitter(data);

        // Feed confidence to updater (if you use it on the Unity side)
        targetPositionUpdater.UpdateConfidence(data.GetConfidence());

        // Hard outlier reject vs. last accepted packet
        if (_lastData != null)
        {
            float posStep = Vector3.Distance(_lastData.GetPosition(), data.GetPosition());
            float rotStep = Quaternion.Angle(_lastData.GetRotation(), data.GetRotation());
            if (posStep > maxStepPosMeters || rotStep > maxStepRotDeg)
            {
                Debug.LogWarning($"[ReceiverProcessor] Hard-reject outlier | Δpos={posStep:0.000} m, Δrot={rotStep:0.0}°");
                return;
            }
        }

        switch (currentMode)
        {
            case ProcessingMode.None:
                ProcessNone(data);
                break;

            case ProcessingMode.BufferInterpolation:
                ProcessBufferInterpolation(data);
                break;

            case ProcessingMode.TimestampCompare:
                ProcessTimestampCompare(data);
                break;

            case ProcessingMode.HybridBuffer:
                ProcessHybridBuffer(data);
                break;
        }

        _lastData = data;
    }

    // ================== MODS ==================

    private void ProcessNone(ReceivedData data)
    {
        targetPositionUpdater.CubePositionSetter(data.GetPosition(), data.GetRotation());
    }

    private void ProcessTimestampCompare(ReceivedData data)
    {
        if (_lastData == null)
        {
            ProcessNone(data);
            return;
        }

        double renderTs = data.timestamp - _predictedDelay;
        double denom = Math.Max(data.timestamp - _lastData.timestamp, 1e-5);
        float t = (float)Mathf.Clamp01((float)((renderTs - _lastData.timestamp) / denom));

        Vector3 pos = Vector3.Lerp(_lastData.GetPosition(), data.GetPosition(), t);
        Quaternion rot = Quaternion.Slerp(_lastData.GetRotation(), data.GetRotation(), t);

        targetPositionUpdater.CubePositionSetter(pos, rot);
    }

    private void ProcessBufferInterpolation(ReceivedData data)
    {
        AppendToBuffer(data);

        double windowSec = ComputeDynamicBufferWindowSeconds();
        double renderTs = data.timestamp - _predictedDelay;

        // find prev <= renderTs < next within window
        ReceivedData prev = null, next = null;

        // iterate from newest to oldest for fast prev selection
        for (int i = _buffer.Count - 1; i >= 0; i--)
        {
            var d = _buffer[i];
            if (data.timestamp - d.timestamp > windowSec) break; // out of window

            if (d.timestamp <= renderTs)
            {
                prev = d;
                // previous of prev (towards newer) might be closer to renderTs as next
                if (i + 1 < _buffer.Count)
                {
                    next = _buffer[i + 1];
                }
                break;
            }
        }

        // If we didn't find prev<=renderTs, try oldest pair within window
        if (prev == null)
        {
            // renderTs is earlier than the oldest in-window packet, so fallback to earliest pair
            for (int i = 0; i + 1 < _buffer.Count; i++)
            {
                var a = _buffer[i];
                var b = _buffer[i + 1];
                if (data.timestamp - a.timestamp <= windowSec)
                {
                    prev = a;
                    next = b;
                    break;
                }
            }
        }

        if (prev != null && next != null && next.timestamp > prev.timestamp)
        {
            double range = next.timestamp - prev.timestamp;
            float t = (float)Mathf.Clamp01((float)((renderTs - prev.timestamp) / Math.Max(range, 1e-5)));
            Vector3 pos = Vector3.Lerp(prev.GetPosition(), next.GetPosition(), t);
            Quaternion rot = Quaternion.Slerp(prev.GetRotation(), next.GetRotation(), t);
            targetPositionUpdater.CubePositionSetter(pos, rot);
        }
        else
        {
            // Fallback: apply latest packet
            targetPositionUpdater.CubePositionSetter(data.GetPosition(), data.GetRotation());
        }
    }

    private void ProcessHybridBuffer(ReceivedData data)
    {
        AppendToBuffer(data);

        double windowSec = ComputeDynamicBufferWindowSeconds();
        double renderTs = data.timestamp - _predictedDelay;

        ReceivedData prev = null, next = null;

        // try to find prev/next around renderTs within window
        for (int i = _buffer.Count - 1; i >= 0; i--)
        {
            var d = _buffer[i];
            if (data.timestamp - d.timestamp > windowSec) break;

            if (d.timestamp <= renderTs)
            {
                prev = d;
                if (i + 1 < _buffer.Count)
                    next = _buffer[i + 1];
                break;
            }
        }

        if (prev != null && next != null && next.timestamp > prev.timestamp)
        {
            // Primary path: buffer-based interpolation
            double range = next.timestamp - prev.timestamp;
            float t = (float)Mathf.Clamp01((float)((renderTs - prev.timestamp) / Math.Max(range, 1e-5)));
            Vector3 pos = Vector3.Lerp(prev.GetPosition(), next.GetPosition(), t);
            Quaternion rot = Quaternion.Slerp(prev.GetRotation(), next.GetRotation(), t);
            targetPositionUpdater.CubePositionSetter(pos, rot);
            return;
        }

        // Secondary path: TimestampCompare between lastData and current packet
        if (_lastData != null)
        {
            double denom = Math.Max(data.timestamp - _lastData.timestamp, 1e-5);
            float t = (float)Mathf.Clamp01((float)((renderTs - _lastData.timestamp) / denom));
            Vector3 pos = Vector3.Lerp(_lastData.GetPosition(), data.GetPosition(), t);
            Quaternion rot = Quaternion.Slerp(_lastData.GetRotation(), data.GetRotation(), t);
            targetPositionUpdater.CubePositionSetter(pos, rot);
            return;
        }

        // Final fallback: apply latest
        targetPositionUpdater.CubePositionSetter(data.GetPosition(), data.GetRotation());
    }

    // ================== SUPPORT ==================

    private void AppendToBuffer(ReceivedData data)
    {
        _buffer.Add(data);

        // Drop very old by dynamic window AND hard-cap total size
        double windowSec = ComputeDynamicBufferWindowSeconds();

        // Remove older than window from the front
        int cutIndex = 0;
        double newestTs = data.timestamp;
        for (int i = 0; i < _buffer.Count; i++)
        {
            if (newestTs - _buffer[i].timestamp <= windowSec)
            {
                cutIndex = i;
                break;
            }
        }
        if (cutIndex > 0) _buffer.RemoveRange(0, cutIndex);

        if (_buffer.Count > MaxBufferedPackets)
            _buffer.RemoveRange(0, _buffer.Count - MaxBufferedPackets);
    }

    private double ComputeDynamicBufferWindowSeconds()
    {
        // Compute window (ms): scale * (delay_ms + jitter_ms) + safety
        double delayMs = _delayEmaMs;
        double windowMs = bufferScale * (delayMs + _jitterEmaMs) + bufferSafetyMs;

        // clamp
        windowMs = Math.Max(minBufferMs, Math.Min(maxBufferMs, windowMs));

        return windowMs / 1000.0;
    }

    private void EstimateCurrentDelayAndJitter(ReceivedData data)
    {
        // Python timestamp is seconds since epoch (UTC)
        double nowSec = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        double delaySec = nowSec - data.timestamp;
        double delayMs = delaySec * 1000.0;

        // EMA for delay (ms) – baseline
        if (_delayEmaMs <= 0)
            _delayEmaMs = delayMs;
        else
            _delayEmaMs = (1.0 - DelaySmoothFactor) * _delayEmaMs + DelaySmoothFactor * delayMs;

        // Jitter as abs deviation from EMA(delay)
        double absDev = Math.Abs(delayMs - _delayEmaMs);
        _jitterEmaMs = (1.0 - jitterEmaAlpha) * _jitterEmaMs + jitterEmaAlpha * absDev;

        // Smoothed predicted delay (seconds) for render timestamp
        _predictedDelay = _predictedDelay * (1 - DelaySmoothFactor) + delaySec * DelaySmoothFactor;

        Debug.Log(
            $"NET Delay: {delayMs:0.0} ms | DelayEMA: {_delayEmaMs:0.0} ms | " +
            $"JitterEMA: {_jitterEmaMs:0.0} ms | RenderDelay: {_predictedDelay * 1000.0:0.0} ms");
    }

    public void SetProcessingMode(int modeIndex)
    {
        currentMode = (ProcessingMode)modeIndex;
        Debug.Log("Processing mode changed to: " + currentMode);
    }

    private void RunThread()
    {
        _receiveThread = new Thread(ReceiveThread) { IsBackground = true };
        _receiveThread.Start();
    }

    private void ReceiveThread()
    {
        while (true)
        {
            var data = _receiver.GetData();
            if (data != null && data.timestamp > _lastTimestamp)
            {
                lock (_receivedDataQueue)
                {
                    _receivedDataQueue.Enqueue(data);
                }
                _lastTimestamp = data.timestamp;
            }
        }
    }

    private void OnDisable()
    {
        _receiver.Close();
        _receiveThread?.Abort();
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        _receiver.Close();
        _receiveThread?.Abort();
        StopAllCoroutines();
    }
}
