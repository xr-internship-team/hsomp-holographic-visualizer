using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    public Transform userTransform;
    public Transform simulationCameraTransform;
    
    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif)
    {
        simulationCameraTransform.rotation = userTransform.rotation * Quaternion.Inverse(rotationDif);
        simulationCameraTransform.position = userTransform.position - simulationCameraTransform.rotation * positionDif;
    }
}
