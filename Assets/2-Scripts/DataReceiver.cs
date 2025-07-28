using UnityEngine;

public class DataReceiver : MonoBehaviour
{
    public GameObject outsideCamera;
    public GameObject marker;
    public TargetPositionUpdater targetPositionUpdater;
    public UdpReceiver receiveData;    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Position Dif:" + outsideCamera.transform.InverseTransformPoint(marker.transform.position));
            Debug.Log("Rotation Dif Euler:" + (Quaternion.Inverse(outsideCamera.transform.rotation) * marker.transform.rotation).eulerAngles);
            Debug.Log("Rotation Dif Quaternion:" + Quaternion.Inverse(outsideCamera.transform.rotation) * marker.transform.rotation);
        }
        
        
        var positionDif = receiveData.receivedPosition;
        var rotationDif = receiveData.receivedRotation;
        targetPositionUpdater.CubePositionSetter(positionDif, rotationDif);
        
    }

    private void Deneme()
    {
        IReceiver receiver = new TcpReceiver("123456",123456);
        receiver.CreateClient("123456",123456);
        
    }
    
    
}
