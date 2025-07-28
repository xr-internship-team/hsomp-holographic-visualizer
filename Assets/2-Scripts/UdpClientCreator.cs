using System;
using System.Net.Sockets;
using UnityEngine;

public class UdpClientCreator: IReceiver
{
    private int _portNumber;
    private UdpClient _client;
    
    public UdpClientCreator(int portNumber)
    {
        _portNumber = portNumber;
        CreateClient(_portNumber);
    }
    
    public void CreateClient(int portNumber)
    {
        _client = new UdpClient(portNumber);
    }
    // Dışarıya okuma izni veren public property
    public UdpClient Client => _client;
}
