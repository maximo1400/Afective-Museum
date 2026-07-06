using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.UI;

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
            targetColor = data.smoothed_valence > 0 ? activeHighColor : activeLowColor;

            float valenceNormalized = Mathf.InverseLerp(-1f, 1f, data.smoothed_valence);
            targetIntensity = Mathf.Lerp(0f, 1f, valenceNormalized);

        } else if (cachedTempleName == "Hovhannes") {
            // Hovhannes: warmer the more relaxed someone is and colder otherwise
            // Aroused = high arousal, Colder color
            // Relaxed = low arousal, Warmer color 
            targetColor = data.smoothed_arousal > 0 ? activeHighColor : activeLowColor;

            targetIntensity = Mathf.Lerp(0f, 1f, Mathf.Abs(data.smoothed_arousal));
        }
        // Debug.Log($"AffectiveLightController: Received Data -> Arousal: {data.smoothed_arousal}, Valence: {data.smoothed_valence} | Target Intensity: {targetIntensity}");
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
