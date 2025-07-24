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

    public GameObject targetObject;
    private Quaternion receivedRotation = Quaternion.identity;

    void Start()
    {
        client = new UdpClient(12345);
        receiveThread = new Thread(ReceiveData);
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

                ReceivedData parsed = JsonUtility.FromJson<ReceivedData>(message);

                if (parsed.rotation_matrix_flat != null && parsed.rotation_matrix_flat.Length == 9)
                {
                    receivedRotation = ConvertFlatMatrixToQuaternion(parsed.rotation_matrix_flat);
                    Debug.Log("Alınan rotasyon: " + receivedRotation.eulerAngles);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("UDP Hatası: " + e.Message);
            }
        }
    }

    void Update()
    {
        if (targetObject != null)
        {
            // Sadece rotasyonu uygula, konumu değiştirme
            targetObject.transform.rotation = receivedRotation;
        }
    }

    Quaternion ConvertFlatMatrixToQuaternion(float[] mat)
    {
        Matrix4x4 m = new Matrix4x4();
        m.m00 = mat[0]; m.m01 = mat[1]; m.m02 = mat[2];
        m.m10 = mat[3]; m.m11 = mat[4]; m.m12 = mat[5];
        m.m20 = mat[6]; m.m21 = mat[7]; m.m22 = mat[8];
        m.m33 = 1f;

        return m.rotation;
    }

    [Serializable]
    public class ReceivedData
    {
        public string timestamp;
        public int id;
        public float[] translation;
        public float[] rotation_matrix_flat;
        public float[] beta_point;
        public float[] alpha_point;
    }
}