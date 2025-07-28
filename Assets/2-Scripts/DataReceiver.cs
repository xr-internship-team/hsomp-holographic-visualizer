using UnityEngine;

public class DataReceiver : MonoBehaviour
{
    public TargetPositionUpdater targetPositionUpdater;
    public UdpDataProcessor receiveData;    
    private void Update()
    {
        var positionDif = receiveData.receivedPosition;
        var rotationDif = receiveData.receivedRotation;
        targetPositionUpdater.CubePositionSetter(positionDif, rotationDif);
        
        /*
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Position Dif:" + outsideCamera.transform.InverseTransformPoint(marker.transform.position));
            Debug.Log("Rotation Dif Euler:" + (Quaternion.Inverse(outsideCamera.transform.rotation) * marker.transform.rotation).eulerAngles);
            Debug.Log("Rotation Dif Quaternion:" + Quaternion.Inverse(outsideCamera.transform.rotation) * marker.transform.rotation);
        }
        */
    }
    
}
