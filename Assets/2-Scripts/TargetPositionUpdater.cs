using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    public Transform markerTransform;
    public Transform objectTransform;
    public Transform testObjectTransform;
    
    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif)
    {
        var originalQuaternion = rotationDif;
        var invertedQuaternion = new Quaternion(
            -originalQuaternion.x,
            -originalQuaternion.y,
            -originalQuaternion.z,
            originalQuaternion.w
        );
        
        var originalVector = positionDif;
        var invertedVector = new Vector3(
            -originalVector.x,
            -originalVector.y,
            -originalVector.z);

        var position = markerTransform.position - objectTransform.rotation * invertedVector;
        var rotaion = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion);
        objectTransform.rotation = rotaion;
        objectTransform.position = position;

        testObjectTransform.rotation = rotaion;
    }
}
