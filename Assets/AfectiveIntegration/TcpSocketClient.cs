using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpSocketClient : MonoBehaviour
{
    [Header("TCP Connection Settings")]
    public string host = "127.0.0.1";
    public int port = 8080;

    private TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isConnected = false;

    void Start()
    {
        ConnectToTcpServer();
    }

    private void ConnectToTcpServer()
    {
        try
        {
            client = new TcpClient(host, port);
            stream = client.GetStream();
            isConnected = true;
            Debug.Log($"Connected to TCP Server at {host}:{port}");

            receiveThread = new Thread(new ThreadStart(ListenForData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError($"Socket exception: {e}");
        }
    }

    private void ListenForData()
    {
        Byte[] bytes = new Byte[1024];
        while (isConnected)
        {
            try
            {
                int length;
                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    var incomingData = new byte[length];
                    Array.Copy(bytes, 0, incomingData, 0, length);
                    string serverMessage = Encoding.UTF8.GetString(incomingData);
                    Debug.Log($"TCP message received: {serverMessage}");
                }
            }
            catch (Exception e)
            {
                if (isConnected) // Only log if we didn't intentionally close
                {
                    Debug.LogError($"Socket exception: {e}");
                }
                break;
            }
        }
    }

    public void SendData(string message)
    {
        if (client == null || !isConnected)
        {
            return;
        }

        try
        {
            byte[] clientMessageAsByteArray = Encoding.UTF8.GetBytes(message);
            stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
            Debug.Log("TCP message sent");
        }
        catch (Exception e)
        {
            Debug.LogError($"Socket exception: {e}");
        }
    }

    void OnDestroy()
    {
        isConnected = false;

        if (stream != null)
        {
            stream.Close();
        }
        if (client != null)
        {
            client.Close();
        }
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        Debug.Log("Disconnected from TCP Server");
    }
}
