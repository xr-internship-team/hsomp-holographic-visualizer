using UnityEngine;

public class DataReceiver : MonoBehaviour
{
    
    public TargetLocationUpdater targetLocationUpdater;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            targetLocationUpdater.UpdateLocation();
        }
    }
}
