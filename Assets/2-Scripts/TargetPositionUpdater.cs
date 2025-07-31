using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    public Transform markerTransform;
    public Transform objectTransform;
    public Transform testObjectTransform;
    
    public float smoothingSpeed = 7f;

    private Vector3 smoothedPosition;
    private Quaternion smoothedRotation;

    private bool isInitialized = false;

    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif)
    {
        var invertedQuaternion = new Quaternion(
            -rotationDif.x,
            -rotationDif.y,
            -rotationDif.z,
            rotationDif.w
        );

        var invertedVector = new Vector3(
            -positionDif.x,
            -positionDif.y,
            -positionDif.z);

        var targetRotation = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion);
        var targetPosition = markerTransform.position - targetRotation * invertedVector;

        if (!isInitialized)
        {
            smoothedPosition = targetPosition;
            smoothedRotation = targetRotation;
            isInitialized = true;
        }
        else
        {
            smoothedPosition = Vector3.Lerp(smoothedPosition, targetPosition, Time.deltaTime * smoothingSpeed);
            smoothedRotation = Quaternion.Slerp(smoothedRotation, targetRotation, Time.deltaTime * smoothingSpeed);
        }

        objectTransform.position = smoothedPosition;
        objectTransform.rotation = smoothedRotation;
        testObjectTransform.rotation = smoothedRotation;

        Debug.Log("STAJ: Smoothed position and rotation set.");
    }
}
