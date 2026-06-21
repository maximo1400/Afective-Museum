using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class AffectiveLightTintController : MonoBehaviour {
    [Header("UI Overlay Settings (For Menus)")]
    public Image TintOverlay;
    [Range(0f, 1f)]
    private readonly float maxOverlayOpacity = 0.85f;

    private readonly float baseIntensity = 1f;
    private readonly float maxIntensityMult = 1f;
    private readonly float minIntensityMult = 0f;

    [Header("Color Settings")]
    private readonly Color transparentColor = new(0f, 0f, 0f, 0f);
    private static string cachedTempleName = "";
    private float targetIntensity;
    private Color targetColor;
    private Color activeHighValColor;
    private Color activeLowValColor;
    private Color activeNeutralColor;

    [Header("Aruch color settings")]
    [SerializeField] private Color aruchHighValColor = new(0.5f, 1f, 0.5f);
    [SerializeField] private Color aruchLowValColor = new(1f, 0.5f, 0.5f);
    [SerializeField] private Color aruchNeutralColor = new(0f, 0f, 0f);

    [Header("Hovhannes color settings")]
    [SerializeField] private Color hovhannesWarmColor = new(1f, 0.5f, 0f);
    [SerializeField] private Color hovhannesColdColor = new(0f, 0.5f, 1f);
    [SerializeField] private Color hovhannesNeutralColor = new(0f, 0f, 0f);


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
        targetIntensity = baseIntensity;
        targetColor = transparentColor;
        UpdateColorCache();

        // Subscribe to AffectiveManager if it exists
        if (AffectiveManager.Instance != null) {
            // Debug.Log("AffectiveLightController: Successfully subscribed to AffectiveManager.");
            AffectiveManager.Instance.OnEmotionDataReceived.AddListener(UpdateLightingParameters);
        } else {
            Debug.LogError("AffectiveLightController: AffectiveManager.Instance is NULL during Start! Cannot subscribe.");
        }
    }

    private void UpdateColorCache() {
        cachedTempleName = AffectiveManager.currentTempleName;
        activeHighValColor = transparentColor;
        activeLowValColor = transparentColor;
        activeNeutralColor = transparentColor;
        if (!AffectiveManager.IsAffectiveSceneActive) return;

        if (cachedTempleName == "Aruch") {
            activeHighValColor = aruchHighValColor;
            activeLowValColor = aruchLowValColor;
            activeNeutralColor = aruchNeutralColor;

        } else if (cachedTempleName == "Hovhannes") {
            activeHighValColor = hovhannesWarmColor;
            activeLowValColor = hovhannesColdColor;
            activeNeutralColor = hovhannesNeutralColor;
        }
    }

    private void UpdateLightingParameters(TcpSocketClient.EmotionData data) {
        if (!AffectiveManager.IsAffectiveSceneActive || AffectiveManager.currentTempleName == "") {
            targetColor = transparentColor;
            targetIntensity = 0f;
            return;
        }

        if (cachedTempleName != AffectiveManager.currentTempleName) {
            UpdateColorCache();
        }

        if (cachedTempleName == "Aruch") {
            // Aruch: darker/moodier in an inverse way to emotions
            // Negative emotions -> tint gets closer to inexistent (0 intensity)
            // Positive emotions -> gets darker (higher intensity)
            float valenceNormalized = Mathf.InverseLerp(-1f, 1f, data.smoothed_valence);
            targetIntensity = Mathf.Lerp(0f, maxIntensityMult, valenceNormalized);

            if (data.smoothed_valence > 0) {
                targetColor = Color.Lerp(activeNeutralColor, activeHighValColor, data.smoothed_valence);
            } else {
                targetColor = Color.Lerp(activeNeutralColor, activeLowValColor, -data.smoothed_valence);
            }

        } else if (cachedTempleName == "Hovhannes") {
            // Hovhannes: warmer the more relaxed someone is and colder otherwise
            // Relaxed = low arousal. Warmer color for arousal < 0, colder color for arousal > 0
            targetIntensity = Mathf.Lerp(0.3f, maxIntensityMult, Mathf.Abs(data.smoothed_arousal));

            if (data.smoothed_arousal > 0) {
                // Excited/Not relaxed: lerp to Cold
                targetColor = Color.Lerp(activeNeutralColor, activeLowValColor, data.smoothed_arousal);
            } else {
                // Relaxed: lerp to Warm
                targetColor = Color.Lerp(activeNeutralColor, activeHighValColor, -data.smoothed_arousal);
            }

        }
        // Debug.Log($"AffectiveLightController: Received Data -> Arousal: {data.smoothed_arousal}, Valence: {data.smoothed_valence} | Target Intensity: {targetIntensity}");
    }

    void Update() {
        if (TintOverlay == null) return;

        Color overlayColor = targetColor;
        // Map arousal to opacity (higher arousal = stronger tint)
        overlayColor.a = Mathf.Lerp(0f, maxOverlayOpacity, targetIntensity / maxIntensityMult);
        TintOverlay.color = Color.Lerp(TintOverlay.color, overlayColor, Time.deltaTime * 2f);


    }

    private void OnDestroy() {
        if (AffectiveManager.Instance != null) {
            AffectiveManager.Instance.OnEmotionDataReceived.RemoveListener(UpdateLightingParameters);
        }
    }
}
