using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpReceiver : IReceiver
{

    private int _portNumber;
    private string _ipAddress;
    private UdpClient _client;

    public UdpReceiver(string ipAddress, int portNumber)
    {
        _ipAddress = ipAddress;
        _portNumber = portNumber;
    }

    public ReceivedData GetData()
    {
        var remoteEndPoint = new IPEndPoint(IPAddress.Any, 12345);

        try
        {
            byte[] data = _client.Receive(ref remoteEndPoint);
            string message = Encoding.UTF8.GetString(data);

            ReceivedData parsed = JsonUtility.FromJson<ReceivedData>(message);
            return parsed;
        }
        catch (Exception e)
        {
            Debug.LogError("UDP Hatası: " + e.Message);
            return null;
        }
        
    }
    #region PrivateFunctions

    public void CreateClient()
    {
        _client = new UdpClient(_portNumber);
    }

    public void CloseClient()
    {
        _client?.Close();
    }
    #endregion
}