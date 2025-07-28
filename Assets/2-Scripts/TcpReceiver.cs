using System;
using System.Net.Sockets;
using UnityEngine;

public class TcpReceiver: IReceiver
{
    private int _portNumber;
    private string _ipAddress;
    private UdpClient _client;
    
    public TcpReceiver(string ipAddress, int portNumber)
    {
        _ipAddress = ipAddress;
        _portNumber = portNumber;
        CreateClient(_ipAddress,_portNumber);
    }
    
    public void CreateClient(string ipAddress, int portNumber)
    {
        _client = new UdpClient(ipAddress,portNumber);
    }
}
