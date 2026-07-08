using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AffectiveManager : MonoBehaviour {
    [System.Serializable]
    public class EmotionDataEvent : UnityEvent<TcpSocketClient.EmotionData> { }

    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }

    [Header("Emotion Events")]
    public EmotionDataEvent OnEmotionDataReceived;
    public FloatEvent OnValenceChanged;
    public FloatEvent OnArousalChanged;

    [Header("Threshold Events")]
    public UnityEvent OnHighArousal;
    public UnityEvent OnNegativeValence;

    [Header("Settings")]
    [SerializeField] private float highArousalThreshold = 0.5f;
    [SerializeField] private float negativeValenceThreshold = -0.5f;

    private TcpSocketClient tcpClient;
    private double lastTimestamp = -1.0;

    public static AffectiveManager Instance { get; private set; }
    public static bool IsAffectiveSceneActive { get; private set; }
    public static string currentTempleName = "";

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            IsAffectiveSceneActive = IsAffectiveScene(SceneManager.GetActiveScene().name);
        } else {
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        if (Instance == this) {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode) {
        IsAffectiveSceneActive = IsAffectiveScene(scene.name);
    }

    public static bool IsAffectiveScene(string sceneName) {
        if (string.IsNullOrEmpty(sceneName)) return false;
        string lowerName = sceneName.ToLower();
        return !(lowerName.Contains("mainmenu") || lowerName.Contains("book"));
    }

    private void Start() {
        tcpClient = FindFirstObjectByType<TcpSocketClient>();
        if (tcpClient == null) {
            Debug.LogWarning("AffectiveManager: No TcpSocketClient found in the scene.");
        }
    }

    private void Update() {
        if (tcpClient == null) return;

        var data = tcpClient.LatestData;
        if (data != null && data.timestamp != lastTimestamp) {
            lastTimestamp = data.timestamp;

            // Fire generic data events
            OnEmotionDataReceived?.Invoke(data);
            OnValenceChanged?.Invoke(data.valence);
            OnArousalChanged?.Invoke(data.arousal);

            // Fire threshold events
            if (data.arousal > highArousalThreshold) {
                OnHighArousal?.Invoke();
            }

            if (data.valence < negativeValenceThreshold) {
                OnNegativeValence?.Invoke();
            }
        }
    }
}
