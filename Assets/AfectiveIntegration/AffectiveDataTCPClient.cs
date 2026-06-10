using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpSocketClient : MonoBehaviour
{
    [Header("TCP Connection Settings")]
    public string host = "127.0.0.1";
    public int port = 65432;
    public float reconnectDelay = 2f;

    [System.Serializable]
    public class EmotionData
    {
        public float raw_valence;
        public float raw_arousal;
        public float smoothed_valence;
        public float smoothed_arousal;
        public float confidence;
        public float timestamp;
    }

    public EmotionData latestData;
    private readonly object dataLock = new object();

    public EmotionData LatestData
    {
        get
        {
            lock (dataLock)
            {
                return latestData;
            }
        }
    }

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientThread;
    private bool isRunning = false;
    private bool isConnected = false;
    public bool IsConnected => isConnected;

    void Start()
    {
        isRunning = true;
        clientThread = new Thread(ClientLoop)
        {
            IsBackground = true
        };
        clientThread.Start();
    }

    private void ClientLoop()
    {
        while (isRunning)
        {
            try
            {
                client = new TcpClient(host, port);
                stream = client.GetStream();
                isConnected = true;
                Debug.Log($"Connected to TCP Server at {host}:{port}");

                Byte[] bytes = new Byte[1024];
                while (isRunning && isConnected)
                {
                    int length = stream.Read(bytes, 0, bytes.Length);
                    
                    // If length is 0, the server closed the connection
                    if (length == 0)
                    {
                        Debug.Log("TCP Server disconnected.");
                        break;
                    }

                    var incomingData = new byte[length];
                    Array.Copy(bytes, 0, incomingData, 0, length);
                    string serverMessage = Encoding.UTF8.GetString(incomingData);

                    EmotionData data = JsonUtility.FromJson<EmotionData>(serverMessage);
                    lock (dataLock)
                    {
                        latestData = data;
                    }
                    Debug.Log($"TCP message received: {serverMessage}");
                }
            }
            catch (Exception)
            {
                // Suppressing socket exception spam on connection failure
            }
            finally
            {
                isConnected = false;
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
                if (client != null)
                {
                    client.Close();
                    client = null;
                }

                if (isRunning)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(reconnectDelay));
                }
            }
        }
    }

    void OnDestroy()
    {
        isRunning = false;
        isConnected = false;

        if (stream != null)
        {
            stream.Close();
        }
        if (client != null)
        {
            client.Close();
        }
        if (clientThread != null && clientThread.IsAlive)
        {
            clientThread.Abort();
        }
    }
}
