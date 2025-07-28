using UnityEngine;

public class DataReceiver : MonoBehaviour
{
    public GameObject outsideCamera;
    public GameObject marker;
    public TargetPositionUpdater targetPositionUpdater;
    public UdpReceiver receiveData;

    void Start()
    {
        receiveData = new UdpReceiver("127.0.0.1", 12345);
    }

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

    private void OnDisable()
    {
        receiveData.StopReceiving();
    }


}
