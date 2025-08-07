using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    public Transform markerTransform;
    public Transform objectTransform;

    private Vector3 _smoothedPosition;
    private Quaternion _smoothedRotation;
    private bool _firstUpdate = true;
    public float smoothFactor;

    private Vector3 _alignmentOffset = Vector3.zero;
    private Quaternion _rotationOffset = Quaternion.identity;
    private bool _useAlignmentOffset = false;  

    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif)
    {
        var invertedVector = new Vector3(-positionDif.x, -positionDif.y, -positionDif.z);
        var invertedQuaternion = new Quaternion(-rotationDif.x, -rotationDif.y, -rotationDif.z, rotationDif.w);

        Vector3 position;
        Quaternion rotation;

        if (_useAlignmentOffset)
        {
            position = markerTransform.position - objectTransform.rotation * invertedVector + _alignmentOffset;
            rotation = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion) * _rotationOffset;
        }
        else
        {
            position = markerTransform.position - objectTransform.rotation * invertedVector;
            rotation = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion);
        }

        if (_firstUpdate)
        {
            _smoothedPosition = position;
            _smoothedRotation = rotation;
            _firstUpdate = false;
        }
        else
        {
            _smoothedPosition = Vector3.Lerp(_smoothedPosition, position, smoothFactor);
            _smoothedRotation = Quaternion.Slerp(_smoothedRotation, rotation, smoothFactor);
        }

        objectTransform.position = _smoothedPosition;
        objectTransform.rotation = _smoothedRotation;
    }

    public void SetSmoothFactor(float value)
    {
        smoothFactor = value;
        Debug.Log($"Smooth factor set to {smoothFactor}");
    }

    public void CalculateAlignmentOffsetTo(Vector3 targetWorldPosition, Quaternion targetWorldRotation)
    {
        // En son hesaplanan VR tabanlı konum
        var currentLocalPos = _smoothedPosition;
        var currentLocalRot = _smoothedRotation;

        _alignmentOffset = targetWorldPosition - currentLocalPos;
        _rotationOffset = targetWorldRotation * Quaternion.Inverse(currentLocalRot);

        _useAlignmentOffset = true;
        Debug.Log("TargetPositionUpdater: Alignment offset calculated from current state.");
    }

}
