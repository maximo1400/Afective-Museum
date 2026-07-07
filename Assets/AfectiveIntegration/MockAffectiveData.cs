using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MockAffectiveData : MonoBehaviour {
    [Header("Simulated Emotion Data")]
    [Range(-1f, 1f)]
    [SerializeField] private float simulatedValence = 0f;
    [Range(-1f, 1f)]
    [SerializeField] private float simulatedArousal = 0f;
    [Range(0f, 1f)]
    public float simulatedConfidence = 1f;

    [Header("Settings")]
    [SerializeField] private bool isEnabled = false;
    private readonly float updateInterval = 0.125f; // 8Hz

    private float timer = 0f;
    private TcpSocketClient TcpClient;
    private GUIStyle guiStyle;

    void Start() {
        TcpClient = FindFirstObjectByType<TcpSocketClient>();
    }

    void Update() {
        if (!isEnabled) return;

        // Keyboard controls for simulated data using the new Input System
        float step = 0.5f;
        if (Keyboard.current != null) {
            if (Keyboard.current.uKey.wasPressedThisFrame)
                simulatedValence = Mathf.Clamp(simulatedValence + step, -1f, 1f);
            if (Keyboard.current.jKey.wasPressedThisFrame)
                simulatedValence = Mathf.Clamp(simulatedValence - step, -1f, 1f);
            if (Keyboard.current.iKey.wasPressedThisFrame)
                simulatedArousal = Mathf.Clamp(simulatedArousal + step, -1f, 1f);
            if (Keyboard.current.kKey.wasPressedThisFrame)
                simulatedArousal = Mathf.Clamp(simulatedArousal - step, -1f, 1f);
        }

        timer += Time.deltaTime;
        if (timer >= updateInterval) {
            timer = 0f;

            // Create a fake EmotionData object
            var data = new TcpSocketClient.EmotionData {
                valence = simulatedValence,
                arousal = simulatedArousal,
                confidence = simulatedConfidence,
                timestamp = Time.time
            };

            if (TcpClient != null) {
                TcpClient.latestData = data;
            }
        }
    }

    void OnGUI() {
        if (!isEnabled || !AffectiveManager.IsAffectiveSceneActive) return;

        // Cache the style so we don't create a new object every frame
        if (guiStyle == null) {
            guiStyle = new GUIStyle(GUI.skin.label);
            guiStyle.fontSize = 24;
            guiStyle.normal.textColor = Color.white;
            guiStyle.alignment = TextAnchor.UpperLeft;
        }

        GUILayout.BeginArea(new Rect(Screen.width - 300, 10, 400, 200));
        GUILayout.Label("Affective Data Controls:", guiStyle);
        GUILayout.Label("U / J : Adjust Valence (" + simulatedValence.ToString("F1") + ")", guiStyle);
        GUILayout.Label("I / K : Adjust Arousal (" + simulatedArousal.ToString("F1") + ")", guiStyle);
        GUILayout.EndArea();
    }
}
