using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    public Transform userTransform;
    public Transform simulationCameraTransform;
    
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

        simulationCameraTransform.rotation = userTransform.rotation * Quaternion.Inverse(invertedQuaternion);
        simulationCameraTransform.position = userTransform.position - simulationCameraTransform.rotation * invertedVector;
    }
}
