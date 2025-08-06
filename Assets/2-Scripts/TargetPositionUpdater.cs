using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class TargetPositionUpdater : MonoBehaviour
{
    public Transform markerTransform;
    public Transform objectTransform;

    [Tooltip("The delay in seconds to render behind the latest received data. Hides network jitter.")]
    public float interpolationDelay = 0.1f; // 100ms delay

    // A buffer to store the incoming states from the network
    private readonly List<ReceivedData> _snapshotBuffer = new List<ReceivedData>();

    void Update()
    {
        // If we don't have enough data to interpolate, do nothing.
        if (_snapshotBuffer.Count < 2)
        {
            return;
        }

        Debug.Log(_snapshotBuffer.Count());
        // The point in time we want to render the object at.
        // We use UtcNow because the Python timestamp is likely a UTC Unix timestamp.
        DateTime renderTime = DateTime.UtcNow - TimeSpan.FromSeconds(interpolationDelay);

        // Find the two snapshots in our buffer that bracket the renderTime.
        ReceivedData snapshotA = null;
        ReceivedData snapshotB = null;

        for (int i = _snapshotBuffer.Count - 1; i >= 0; i--)
        {
            if (_snapshotBuffer[i].TimestampAsDateTime <= renderTime)
            {
                snapshotA = _snapshotBuffer[i];
                if (i + 1 < _snapshotBuffer.Count)
                {
                    snapshotB = _snapshotBuffer[i + 1];
                }
                break;
            }
        }

        // If we have two valid snapshots, we can interpolate.
        if (snapshotA != null && snapshotB != null)
        {
            double timeBetweenSnapshots = (snapshotB.TimestampAsDateTime - snapshotA.TimestampAsDateTime).TotalSeconds;
            double timeFromA = (renderTime - snapshotA.TimestampAsDateTime).TotalSeconds;
            
            // Prevent division by zero if timestamps are identical
            if (timeBetweenSnapshots <= 0) return;

            float t = (float)(timeFromA / timeBetweenSnapshots);

            var poseA = CalculateWorldPose(snapshotA);
            var poseB = CalculateWorldPose(snapshotB);

            objectTransform.position = Vector3.Lerp(poseA.position, poseB.position, t);
            objectTransform.rotation = Quaternion.Slerp(poseA.rotation, poseB.rotation, t);
        }
    }

    /// <summary>
    /// This public method is called by ReceiverProcessor whenever a new data packet arrives.
    /// </summary>
    public void OnDataReceived(ReceivedData data)
    {
        _snapshotBuffer.Add(data);
        _snapshotBuffer.Sort((a, b) => a.TimestampAsDateTime.CompareTo(b.TimestampAsDateTime));

        // Optional: Remove very old snapshots to keep the buffer from growing forever.
        if (_snapshotBuffer.Count > 100)
        {
            _snapshotBuffer.RemoveAt(0);
        }
    }

    private (Vector3 position, Quaternion rotation) CalculateWorldPose(ReceivedData data)
    {
        Vector3 positionDif = data.GetPosition();
        Quaternion rotationDif = data.GetRotation();

        var invertedQuaternion = new Quaternion(-rotationDif.x, -rotationDif.y, -rotationDif.z, rotationDif.w);
        var invertedVector = new Vector3(-positionDif.x, -positionDif.y, -positionDif.z);

        Vector3 targetPosition = markerTransform.position - objectTransform.rotation * invertedVector;
        Quaternion targetRotation = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion);

        return (targetPosition, targetRotation);
    }
}