using UnityEngine;

public class DataReceiver : MonoBehaviour
{
    public TargetLocationUpdater targetLocationUpdater; // Referans al

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            targetLocationUpdater.RotateLogger();
        }
    }
}
