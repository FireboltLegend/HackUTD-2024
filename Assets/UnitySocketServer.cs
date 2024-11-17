using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class UnitySocketServer : MonoBehaviour
{
    TcpListener server;
    TcpClient client;
    NetworkStream stream;

    void Start()
    {
        server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Debug.Log("Server started on port 5000.");
        server.BeginAcceptTcpClient(AcceptClient, null);
    }

    void AcceptClient(IAsyncResult result)
    {
        client = server.EndAcceptTcpClient(result);
        stream = client.GetStream();
        Debug.Log("Client connected.");
    }

    void Update()
    {
        if (stream != null && stream.CanWrite)
        {
            string dataToSend = "Hello from Unity! Current Time: " + DateTime.Now;
            byte[] data = Encoding.ASCII.GetBytes(dataToSend);
            stream.Write(data, 0, data.Length);
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
        server?.Stop();
    }
}