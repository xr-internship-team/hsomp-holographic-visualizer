using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// Receives UDP packets and converts them into ReceivedData objects.
public class UdpReceiver: IReceiver
{
    private int _portNumber;
    private UdpClient _client;

    /// Constructor - sets the port number and creates the client.
    public UdpReceiver(int portNumber)
    {
        _portNumber = portNumber;
        CreateClient();
    }

    /// Initializes the UDP client and starts listening on the specified port.
    public void CreateClient()
    {
        _client = new UdpClient(_portNumber);
        Debug.Log("UDP client created on port " + _portNumber);
    }

    /// Waits for incoming UDP data, decodes it from UTF-8, and deserializes it into a ReceivedData object.
    /// Returns ReceivedData object if successful, null if an error occurs.
    public ReceivedData GetData()
    {
        var remoteEndPoint = new IPEndPoint(IPAddress.Any, _portNumber);

        try
        {
            var data = _client.Receive(ref remoteEndPoint);
            var message = Encoding.UTF8.GetString(data);
            var receivedData = JsonUtility.FromJson<ReceivedData>(message);
            return receivedData;
        }
        catch (Exception e)
        {
            Debug.LogError("UDP Error: " + e.Message);
            return null;
        }
    }

    /// Closes the UDP client and releases resources.
    public void Close()
    {
        _client.Close();
    }
}
