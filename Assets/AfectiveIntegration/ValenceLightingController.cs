using UnityEngine;
using UnityEngine.SceneManagement;

public class ValenceLightingController : MonoBehaviour
{
    private Light targetLight;
    private float baseIntensity;
    private TcpSocketClient cachedClient;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnLoad()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Only run for the jose scene
        if (scene.name == "jose")
        {
            // Avoid duplicates if reloaded
            if (UnityEngine.Object.FindAnyObjectByType<ValenceLightingController>() == null)
            {
                GameObject go = new GameObject("ValenceLightingController");
                go.AddComponent<ValenceLightingController>();
            }
        }
    }

    void Start()
    {
        Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(UnityEngine.FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                targetLight = l;
                baseIntensity = l.intensity;
                break;
            }
        }

        if (targetLight == null && RenderSettings.sun != null)
        {
            targetLight = RenderSettings.sun;
            baseIntensity = targetLight.intensity;
        }
    }

    void Update()
    {
        if (targetLight == null) return;

        if (cachedClient == null)
        {
            cachedClient = UnityEngine.Object.FindAnyObjectByType<TcpSocketClient>();
            if (cachedClient == null) return;
        }

        var data = cachedClient.LatestData;
        if (data != null)
        {
            // Valence is usually between -1.0 (negative) and 1.0 (positive)
            // We map -1.0 to 0.1 multiplier (darker) and 1.0 to 1.0 multiplier (normal)
            float valenceNormalized = Mathf.Clamp01((data.smoothed_valence + 1f) / 2f);
            float intensityMult = Mathf.Lerp(0.1f, 1.0f, valenceNormalized);
            
            // Smoothly interpolate current intensity towards the target intensity
            targetLight.intensity = Mathf.Lerp(targetLight.intensity, baseIntensity * intensityMult, Time.deltaTime * 2f);
        }
    }
}
