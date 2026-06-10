using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class AffectiveLightTintController : MonoBehaviour
{
    [Header("UI Overlay Settings (For Menus)")]
    public Image fullScreenOverlay;
    [Range(0f, 1f)]
    public float maxOverlayOpacity = 0.8f; // Massively increased from 0.3f to make it extreme

    [Header("Light Settings (DISABLED)")]
    public Light targetLight;
    public float baseIntensity = 1f;
    public float maxIntensityMult = 10.0f; // Boosted to make it obvious
    public float minIntensityMult = 0.2f;

    [Header("Color Settings")]
    public Color highValenceColor = Color.green; // Extremely obvious
    public Color lowValenceColor = Color.red;    // Extremely obvious
    public Color neutralColor = Color.white;

    private float targetIntensity;
    private Color targetColor;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnLoad()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name != "MainMenu" && scene.name != "Book")
        {
            if (FindAnyObjectByType<AffectiveLightTintController>() == null)
            {
                GameObject go = new GameObject("AffectiveLightTintController");
                go.AddComponent<AffectiveLightTintController>();
            }
        }
    }

    void Start()
    {
        if (targetLight != null)
        {
            baseIntensity = targetLight.intensity;
            targetIntensity = baseIntensity;
        }
        else
        {
            targetIntensity = baseIntensity;
        }

        targetColor = neutralColor;

        // Subscribe to AffectiveManager if it exists
        if (AffectiveManager.Instance != null)
        {
            // Debug.Log("AffectiveLightController: Successfully subscribed to AffectiveManager.");
            AffectiveManager.Instance.OnEmotionDataReceived.AddListener(UpdateLightingParameters);
        }
        else
        {
            Debug.LogError("AffectiveLightController: AffectiveManager.Instance is NULL during Start! Cannot subscribe.");
        }
    }

    private void UpdateLightingParameters(TcpSocketClient.EmotionData data)
    {
        // Map Arousal (-1 to 1) to Intensity multiplier
        float arousalNormalized = Mathf.Clamp01((data.smoothed_arousal + 1f) / 2f);
        float intensityMult = Mathf.Lerp(minIntensityMult, maxIntensityMult, arousalNormalized);
        targetIntensity = baseIntensity * intensityMult;

        // Map Valence (-1 to 1) to Color
        if (data.smoothed_valence > 0)
        {
            targetColor = Color.Lerp(neutralColor, highValenceColor, data.smoothed_valence);
        }
        else
        {
            targetColor = Color.Lerp(neutralColor, lowValenceColor, -data.smoothed_valence);
        }

        // Debug.Log($"AffectiveLightController: Received Data -> Arousal: {data.smoothed_arousal}, Valence: {data.smoothed_valence} | Target Intensity: {targetIntensity}");
    }

    void Update()
    {
        // Only update the UI Overlay now, Lights and Ambient are disabled by request
        if (fullScreenOverlay != null)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "MainMenu" || sceneName == "Book")
            {
                // Keep it completely transparent and colorless in these scenes
                fullScreenOverlay.color = new Color(1f, 1f, 1f, 0f);
                return;
            }

            // Apply the color to the UI overlay but keep it slightly transparent
            Color overlayColor = targetColor;

            // Map arousal to opacity (higher arousal = stronger tint)
            overlayColor.a = Mathf.Lerp(0f, maxOverlayOpacity, targetIntensity / maxIntensityMult);

            fullScreenOverlay.color = Color.Lerp(fullScreenOverlay.color, overlayColor, Time.deltaTime * 2f);
        }
    }

    private void OnDestroy()
    {
        if (AffectiveManager.Instance != null)
        {
            AffectiveManager.Instance.OnEmotionDataReceived.RemoveListener(UpdateLightingParameters);
        }
    }
}
