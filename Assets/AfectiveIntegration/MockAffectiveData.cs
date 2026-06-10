using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

        // Keyboard controls for simulated data using the new Input System
        float step = 0.5f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.uKey.wasPressedThisFrame)
                simulatedValence = Mathf.Clamp(simulatedValence + step, 0f, 4f);
            if (Keyboard.current.jKey.wasPressedThisFrame)
                simulatedValence = Mathf.Clamp(simulatedValence - step, 0f, 4f);
            if (Keyboard.current.iKey.wasPressedThisFrame)
                simulatedArousal = Mathf.Clamp(simulatedArousal + step, 0f, 4f);
            if (Keyboard.current.kKey.wasPressedThisFrame)
                simulatedArousal = Mathf.Clamp(simulatedArousal - step, 0f, 4f);
        }

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

    void OnGUI()
    {
        if (!isEnabled) return;

        string sceneName = SceneManager.GetActiveScene().name.ToLower();
        if (sceneName.Contains("mainmenu") || sceneName.Contains("book")) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;

        GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 400, 200));
        GUILayout.Label("Mock Data Controls:", style);
        GUILayout.Label("U / J : Adjust Valence (" + simulatedValence.ToString("F1") + ")", style);
        GUILayout.Label("I / K : Adjust Arousal (" + simulatedArousal.ToString("F1") + ")", style);
        GUILayout.EndArea();
    }
}
