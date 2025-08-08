using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpReceiver: IReceiver
{
    private int _portNumber;
    private UdpClient _client;
    
    public UdpReceiver(int portNumber)
    {
        _portNumber = portNumber;
        CreateClient();
    }

    public void CreateClient()
    {
        _client = new UdpClient(_portNumber);
        Debug.Log("STAJ: Client created.");
    }

    public ReceivedData GetData()
    {
        var remoteEndPoint = new IPEndPoint(IPAddress.Any, _portNumber);

        try
        {
            var data = _client.Receive(ref remoteEndPoint);
            var message = Encoding.UTF8.GetString(data);
            var receivedData = JsonUtility.FromJson<ReceivedData>(message);
            // Debug.Log($"Alınan Pozisyon: {receivedData.GetPosition()}, Rotasyon: {receivedData.GetRotation()}");
            return receivedData;
        }
        catch (Exception e)
        {
            Debug.LogError("UDP Hatası: " + e.Message);
            return null;
        }
    }

    public void Close()
    {
        _client.Close();
    }
}
