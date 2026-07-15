using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.IO;

public class AffectiveLightTintController : MonoBehaviour {
    [Header("UI Overlay Settings (For Menus)")]
    public Image TintOverlay;
    [Range(0f, 1f)]
    private float maxOverlayOpacity;

    [Header("Color Settings")]
    private static string cachedTempleName;
    private float targetIntensity;
    private Color targetColor;
    private Color activeHighColor;
    private Color activeLowColor;

    [Header("Aruch color settings")]
    private Color aruchHighValColor = Color.black;
    private Color aruchLowValColor = Color.clear;
    private readonly float aruchMaxOpacity = 0.6f;

    [Header("Hovhannes color settings")]
    private Color hovhannesWarmColor = Color.darkOrange;
    private Color hovhannesColdColor = Color.dodgerBlue;
    private readonly float hovhannesMaxOpacity = 0.15f;
    private double unityStartingTimestamp;
    private string sessionStartTimeStr;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnLoad() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) {
        if (AffectiveManager.IsAffectiveScene(scene.name)) {
            if (FindAnyObjectByType<AffectiveLightTintController>() == null) {
                GameObject go = new("AffectiveLightTintController");
                go.AddComponent<AffectiveLightTintController>();
            }
        }
    }



    void Start() {
        unityStartingTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        sessionStartTimeStr = ((long)unityStartingTimestamp).ToString();

        targetColor = Color.clear;
        UpdateColorCache();

        // Subscribe to AffectiveManager if it exists
        if (AffectiveManager.Instance != null) {
            // Debug.Log("AffectiveLightController: Successfully subscribed to AffectiveManager.");
            AffectiveManager.Instance.OnEmotionDataReceived.AddListener(UpdateLightingParameters);
        }
    }

    private void UpdateColorCache() {
        cachedTempleName = AffectiveManager.currentTempleName;
        activeHighColor = Color.clear;
        activeLowColor = Color.clear;
        if (!AffectiveManager.IsAffectiveSceneActive) return;

        if (cachedTempleName == "Aruch") {
            activeHighColor = aruchHighValColor;
            activeLowColor = aruchLowValColor;
            maxOverlayOpacity = aruchMaxOpacity;

        } else if (cachedTempleName == "Hovhannes") {
            activeHighColor = hovhannesWarmColor;
            activeLowColor = hovhannesColdColor;
            maxOverlayOpacity = hovhannesMaxOpacity;
        }
    }

    private void UpdateLightingParameters(TcpSocketClient.EmotionData data) {
        if (!AffectiveManager.IsAffectiveSceneActive || AffectiveManager.currentTempleName == "") {
            targetColor = Color.clear;
            targetIntensity = 0f;
            return;
        }

        if (cachedTempleName != AffectiveManager.currentTempleName) {
            UpdateColorCache();
        }

        if (cachedTempleName == "Aruch") {
            // Aruch: darker/moodier in an inverse way to emotions
            // Positive emotions -> gets darker (night = can't see = danger)
            // Negative emotions -> tint gets closer to inexistent 
            targetColor = data.valence > 0 ? activeHighColor : activeLowColor;

            float valenceNormalized = Mathf.InverseLerp(-1f, 1f, data.valence);
            targetIntensity = Mathf.Lerp(0f, 1f, valenceNormalized);

        } else if (cachedTempleName == "Hovhannes") {
            // Hovhannes: warmer the more relaxed someone is and colder otherwise
            // Aroused = high arousal, Colder color
            // Relaxed = low arousal, Warmer color 
            targetColor = data.arousal > 0 ? activeHighColor : activeLowColor;

            targetIntensity = Mathf.Lerp(0f, 1f, Mathf.Abs(data.arousal));
        }

        LogLightData(data);
        // Debug.Log($"AffectiveLightController: Received Data -> Arousal: {data.arousal}, Valence: {data.valence} | Target Intensity: {targetIntensity}");
    }

    private void LogLightData(TcpSocketClient.EmotionData data) {
        if (!AffectiveManager.IsAffectiveSceneActive) return;

        string baseFolderPath = Path.Combine(Application.dataPath, "../AffectiveReports/");
        string folderPath = Path.Combine(baseFolderPath, $"Session_{sessionStartTimeStr}");

        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
        }

        string reportPath = Path.Combine(folderPath, $"light_out_{sessionStartTimeStr}.csv");
        bool writeHeader = !File.Exists(reportPath);

        using StreamWriter writer = new(reportPath, true);
        if (writeHeader) {
            writer.WriteLine("timestamp,temple,valence,arousal,confidence,data_timestamp,unity_timestamp,data_starting_timestamp,unity_starting_timestamp,target_intensity,target_color_r,target_color_g,target_color_b");
        }
        double currentUnityTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        string templeName = string.IsNullOrEmpty(AffectiveManager.currentTempleName) ? "None" : AffectiveManager.currentTempleName;
        string row = $"{System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss},{templeName},{data.valence},{data.arousal},{data.confidence},{data.timestamp:F10},{currentUnityTimestamp:F10},{data.starting_timestamp:F10},{unityStartingTimestamp:F10},{targetIntensity},{targetColor.r},{targetColor.g},{targetColor.b}";
        writer.WriteLine(row);
    }

    void Update() {
        if (TintOverlay == null) return;

        Color overlayColor = targetColor;
        overlayColor.a = Mathf.Lerp(0f, maxOverlayOpacity, targetIntensity);
        TintOverlay.color = Color.Lerp(TintOverlay.color, overlayColor, Time.deltaTime * 2f);
    }

    private void OnDestroy() {
        if (AffectiveManager.Instance != null) {
            AffectiveManager.Instance.OnEmotionDataReceived.RemoveListener(UpdateLightingParameters);
        }
    }
}
