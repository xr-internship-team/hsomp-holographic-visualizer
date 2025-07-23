using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject target;
    
    
    public void CubePositionSetter(Vector3 refPosition, Vector3 refRotation)
    {
        var myPosition = mainCamera.transform.position;
        var outsideCameraPosition = myPosition - refPosition;
        
        target.transform.position = outsideCameraPosition;
        target.SetActive(true);
    }
}
