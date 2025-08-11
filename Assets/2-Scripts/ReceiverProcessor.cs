using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ReceiverProcessor : MonoBehaviour
{
    public enum ProcessingMode
    {
        None,
        AdaptivePrediction,
        TimestampCompare,
        BufferInterpolation
    }

    public ProcessingMode currentMode = ProcessingMode.None;
    public TargetPositionUpdater targetPositionUpdater;

    private IReceiver _receiver;
    private Thread _receiveThread;
    private Queue<ReceivedData> _receivedDataQueue = new(40);
    private double _lastTimestamp;

    // Dinamik delay hesaplama
    private double _predictedDelay = 0.03; // saniye
    private const double DelaySmoothFactor = 0.1; // smoothing katsayısı
    private ReceivedData _lastData;

    // BufferInterpolation için
    private readonly List<ReceivedData> _buffer = new();
    private const double BufferSize = 0.1; // saniye

    private void Start()
    {
        _receiver = new UdpReceiver(12345);
        RunThread();
    }

    private void Update()
    {
        lock (_receivedDataQueue)
        {
            if (_receivedDataQueue.Count > 0)
            {
                var data = _receivedDataQueue.Dequeue();

                // Delay ölçümü
                EstimateCurrentDelay(data);

                switch (currentMode)
                {
                    case ProcessingMode.None:
                        ProcessNone(data);
                        break;
                    case ProcessingMode.AdaptivePrediction:
                        ProcessAdaptivePrediction(data);
                        break;
                    case ProcessingMode.TimestampCompare:
                        ProcessTimestampCompare(data);
                        break;
                    case ProcessingMode.BufferInterpolation:
                        ProcessBufferInterpolation(data);
                        break;
                }

                _lastData = data;
            }
        }
    }

    private void ProcessNone(ReceivedData data)
    {
        targetPositionUpdater.CubePositionSetter(data.GetPosition(), data.GetRotation());
    }

    private void ProcessAdaptivePrediction(ReceivedData data)
    {
        if (_lastData == null)
        {
            ProcessNone(data);
            return;
        }

        double dt = data.timestamp - _lastData.timestamp;
        if (dt <= 0.00001 || dt > 0.2f)
        {
            ProcessNone(data);
            return;
        }

        // Velocity hesapla
        Vector3 velocity = (data.GetPosition() - _lastData.GetPosition()) / (float)dt;

        // Maks hız limiti (ani zıplamaları engellemek için)
        float maxSpeed = 2.0f;
        if (velocity.magnitude > maxSpeed)
            velocity = velocity.normalized * maxSpeed;

        // Dinamik delay ile tahmin
        Vector3 predictedPosition = data.GetPosition() + velocity * (float)_predictedDelay;
        Quaternion predictedRotation = data.GetRotation();

        targetPositionUpdater.CubePositionSetter(predictedPosition, predictedRotation);
    }

    private void ProcessTimestampCompare(ReceivedData data)
    {
        if (_lastData == null)
        {
            ProcessNone(data);
            return;
        }

        double targetTimestamp = data.timestamp - _predictedDelay;
        if (Math.Abs(_lastData.timestamp - targetTimestamp) < 0.0001)
        {
            ProcessNone(data);
            return;
        }

        // İki veri arasında linear interpolation
        Vector3 interpolatedPos = Vector3.Lerp(_lastData.GetPosition(), data.GetPosition(), 0.5f);
        Quaternion interpolatedRot = Quaternion.Slerp(_lastData.GetRotation(), data.GetRotation(), 0.5f);

        targetPositionUpdater.CubePositionSetter(interpolatedPos, interpolatedRot);
    }

    private void ProcessBufferInterpolation(ReceivedData data)
    {
        _buffer.Add(data);

        // Eski verileri temizle
        _buffer.RemoveAll(d => data.timestamp - d.timestamp > BufferSize);

        double renderTimestamp = data.timestamp - _predictedDelay;
        ReceivedData prev = null, next = null;

        foreach (var d in _buffer)
        {
            if (d.timestamp <= renderTimestamp) prev = d;
            if (d.timestamp > renderTimestamp)
            {
                next = d;
                break;
            }
        }

        if (prev != null && next != null)
        {
            double range = next.timestamp - prev.timestamp;
            double t = (renderTimestamp - prev.timestamp) / range;
            Vector3 pos = Vector3.Lerp(prev.GetPosition(), next.GetPosition(), (float)t);
            Quaternion rot = Quaternion.Slerp(prev.GetRotation(), next.GetRotation(), (float)t);
            targetPositionUpdater.CubePositionSetter(pos, rot);
        }
        else
        {
            targetPositionUpdater.CubePositionSetter(data.GetPosition(), data.GetRotation());
        }
    }

    private void EstimateCurrentDelay(ReceivedData data)
    {
        // Burada Python'dan gelen timestamp Unix epoch saniye cinsinden
        double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        double networkDelay = now - data.timestamp;

        // Low-pass filter ile smoothing
        _predictedDelay = _predictedDelay * (1 - DelaySmoothFactor) + networkDelay * DelaySmoothFactor;

        Debug.Log($"UDP Delay: {networkDelay * 1000:F2} ms | Smoothed: {_predictedDelay * 1000:F2} ms");
    }

    public void SetProcessingMode(int modeIndex)
    {
        currentMode = (ProcessingMode)modeIndex;
        Debug.Log("Processing mode changed to: " + currentMode);
    }

    private void RunThread()
    {
        _receiveThread = new Thread(ReceiveThread);
        _receiveThread.IsBackground = true;
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
