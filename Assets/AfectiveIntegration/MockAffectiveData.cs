using UnityEngine;

public class MockAffectiveData : MonoBehaviour
{
    [Header("Simulated Emotion Data")]
    [Range(0f, 4f)]
    public float simulatedValence = 0f;
    [Range(0f, 4f)]
    public float simulatedArousal = 0f;
    [Range(0f, 1f)]
    public float simulatedConfidence = 1f;

    [Header("Settings")]
    public bool isEnabled = false;
    public float updateInterval = 0.125f; // 8Hz

    private float timer = 0f;
    private TcpSocketClient TcpClient;

    void Start()
    {
        TcpClient = FindFirstObjectByType<TcpSocketClient>();
    }

    void Update()
    {
        if (!isEnabled) return;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            
            // Create a fake EmotionData object
            var data = new TcpSocketClient.EmotionData
            {
                raw_valence = simulatedValence,
                raw_arousal = simulatedArousal,
                smoothed_valence = simulatedValence,
                smoothed_arousal = simulatedArousal,
                confidence = simulatedConfidence,
                timestamp = Time.time
            };

            if (TcpClient != null)
            {
                TcpClient.latestData = data;
            }
        }
    }
}
