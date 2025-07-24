using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpReceiver : MonoBehaviour
{
    UdpClient client;
    Thread receiveThread;

    public Vector3 receivedPosition;
    public Quaternion receivedRotation;

    void Start()
    {
        client = new UdpClient(12345);
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 12345);
        while (true)
        {
            try
            {
                byte[] data = client.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(data);

                PoseData pose = JsonUtility.FromJson<PoseData>(message);
                receivedPosition = new Vector3(pose.position[0], pose.position[1], pose.position[2]);
                receivedRotation = new Quaternion(pose.rotation[0], pose.rotation[1], pose.rotation[2], pose.rotation[3]);

                Debug.Log("Alındı: " + receivedPosition);
            }
            catch (Exception e)
            {
                Debug.LogError("UDP Hatası: " + e.Message);
            }
        }
    }

    [Serializable]
    public class PoseData
    {
        public float[] position;
        public float[] rotation;
    }
}