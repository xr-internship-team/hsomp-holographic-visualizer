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
            var referencePosition = outsideCamera.transform.InverseTransformPoint(marker.transform.position);
            var referenceRotation = Vector3.zero;
            targetPositionUpdater.CubePositionSetter(referencePosition, referenceRotation);
        }
    }
}
