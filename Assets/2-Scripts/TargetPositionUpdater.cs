using UnityEngine;
using UnityEngine.Serialization;

public class TargetPositionUpdater : MonoBehaviour
{
    public Transform markerTransform;
    public Transform objectTransform;
    
    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif)
    {
        Quaternion originalQuaternion = rotationDif;
        Quaternion invertedQuaternion = new Quaternion(
            -originalQuaternion.x,
            -originalQuaternion.y,
            -originalQuaternion.z,
            originalQuaternion.w
        );
        
        Vector3 originalVector = positionDif;
        Vector3 invertedVector = new Vector3(
            -originalVector.x,
            -originalVector.y,
            -originalVector.z);

        objectTransform.rotation = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion);
        objectTransform.position = markerTransform.position - objectTransform.rotation * invertedVector;
    }
}
