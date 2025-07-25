using UnityEngine;

public class DataReceiver : MonoBehaviour
{
    public GameObject outsideCamera;
    public GameObject marker;
    public TargetPositionUpdater targetPositionUpdater;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Position Dif:" + outsideCamera.transform.InverseTransformPoint(marker.transform.position));
            Debug.Log("Rotation Dif Euler:" + (Quaternion.Inverse(outsideCamera.transform.rotation) * marker.transform.rotation).eulerAngles);
            Debug.Log("Rotation Dif Quaternion:" + Quaternion.Inverse(outsideCamera.transform.rotation) * marker.transform.rotation);
        }
        
        var positionDif = outsideCamera.transform.InverseTransformPoint(marker.transform.position);
        var rotationDif = Quaternion.Inverse(outsideCamera.transform.rotation) * marker.transform.rotation;
        targetPositionUpdater.CubePositionSetter(positionDif, rotationDif);
    }
}
