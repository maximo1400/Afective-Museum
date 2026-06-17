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
    public static string currentTempleName = "";
    private static string cachedTempleName = "";
    private float targetIntensity;
    private Color targetColor;
    private Color activeHighValColor;
    private Color activeLowValColor;
    private Color activeNeutralColor;

    [Header("Aruch color settings")]
    [SerializeField] private Color aruchHighValColor = new(0.5f, 1f, 0.5f);
    [SerializeField] private Color aruchLowValColor = new(1f, 0.5f, 0.5f);
    [SerializeField] private Color aruchNeutralColor = new(0f, 0f, 0f); // Changed from white to black

    [Header("Hovhannes color settings")]
    [SerializeField] private Color hovhannesHighValColor = new(0.5f, 1f, 0.5f);
    [SerializeField] private Color hovhannesLowValColor = new(1f, 0.5f, 0.5f);
    [SerializeField] private Color hovhannesNeutralColor = new(0f, 0f, 0f); // Changed from white to black



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
        cachedTempleName = currentTempleName;

        if (!AffectiveManager.IsAffectiveSceneActive) {
            activeHighValColor = transparentColor;
            activeLowValColor = transparentColor;
            activeNeutralColor = transparentColor;

        } else if (currentTempleName == "Aruch") {
            activeHighValColor = aruchHighValColor;
            activeLowValColor = aruchLowValColor;
            activeNeutralColor = aruchNeutralColor;

        } else if (currentTempleName == "Hovhannes") {
            activeHighValColor = hovhannesHighValColor;
            activeLowValColor = hovhannesLowValColor;
            activeNeutralColor = hovhannesNeutralColor;

        } else {
            // disable tint in non-temple scenes
            activeHighValColor = transparentColor;
            activeLowValColor = transparentColor;
            activeNeutralColor = transparentColor;
        }
    }

    private void UpdateLightingParameters(TcpSocketClient.EmotionData data) {
        if (!AffectiveManager.IsAffectiveSceneActive || currentTempleName == "") {
            targetColor = transparentColor;
            targetIntensity = 0f;
            return;
        }
        // Map Arousal (-1 to 1) to a 0 to 1 range for Lerp
        float arousalNormalized = Mathf.InverseLerp(-1f, 1f, data.smoothed_arousal);

        // Map Arousal to Intensity multiplier. 
        // We multiply by the absolute value of valence to fade out the tint as valence approaches 0.
        float intensityMult = Mathf.Lerp(minIntensityMult, maxIntensityMult, arousalNormalized);
        targetIntensity = intensityMult * Mathf.Abs(data.smoothed_valence);

        if (cachedTempleName != currentTempleName) {
            UpdateColorCache();
        }

        // Map Valence (-1 to 1) to Color
        if (data.smoothed_valence > 0) {
            targetColor = Color.Lerp(activeNeutralColor, activeHighValColor, data.smoothed_valence);
        } else {
            targetColor = Color.Lerp(activeNeutralColor, activeLowValColor, -data.smoothed_valence);
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
