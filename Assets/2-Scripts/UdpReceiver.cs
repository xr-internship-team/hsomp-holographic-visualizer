using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpReceiver : MonoBehaviour
{
    public GameObject targetObject;
    public Quaternion receivedRotation = Quaternion.identity;
    public Vector3 receivedPosition = Vector3.zero;
    
    private UdpClient _client;
    private Thread _receiveThread;

    #region UnityEventFunctions
    private void Start()
    {
        _client = new UdpClient(12345);
        _receiveThread = new Thread(ReceiveData);
        _receiveThread.IsBackground = true;
        _receiveThread.Start();
    }
    
    private void Update()
    {
        targetObject.transform.position = receivedPosition;
        targetObject.transform.rotation = receivedRotation;
    }

    private void OnDisable()
    {
        _receiveThread.Abort();
    }

    private void OnDestroy()
    {
        throw new NotImplementedException();
    }

    #endregion
    
    #region PrivateFunctions
    private void ReceiveData()
    {
        var remoteEndPoint = new IPEndPoint(IPAddress.Any, 12345);
        while (true)
        {
            try
            {
                byte[] data = _client.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(data);

                ReceivedData parsed = JsonUtility.FromJson<ReceivedData>(message);

                if (parsed.quaternion != null && parsed.quaternion.Length == 4)
                {
                    receivedRotation = new Quaternion(
                        parsed.quaternion[0],
                        parsed.quaternion[1],
                        parsed.quaternion[2],
                        parsed.quaternion[3]
                    );
                }

                if (parsed.translation != null && parsed.translation.Length == 3)
                {
                    receivedPosition = new Vector3(
                        parsed.translation[0],
                        parsed.translation[1],
                        parsed.translation[2]
                    );
                }

                Debug.Log($"Alınan Pozisyon: {receivedPosition}, Rotasyon: {receivedRotation.eulerAngles}");
            }
            catch (Exception e)
            {
                Debug.LogError("UDP Hatası: " + e.Message);
            }
        }
    }
    #endregion
}