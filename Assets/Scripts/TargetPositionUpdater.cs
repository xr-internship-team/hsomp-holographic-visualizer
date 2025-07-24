using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject target;
    
    
    public void CubePositionSetter(Vector3 refPosition, Vector3 refRotation)
    {
        var myPosition = mainCamera.transform.position;

        var rotation = Quaternion.Euler(refRotation);
        var outsideCameraPosition = (rotation * -refPosition)+myPosition;

        target.transform.position = outsideCameraPosition;

        target.SetActive(true);
    }
}
