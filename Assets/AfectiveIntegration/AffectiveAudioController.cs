using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AffectiveAudioController : MonoBehaviour {
    [Header("Audio Sources")]
    [Tooltip("The main AudioSource on this object, used for global/positive valence music.")]
    public AudioSource globalAudioSource;
    [Tooltip("A secondary AudioSource, used for scary/negative valence music (Hovhanes only).")]
    public AudioSource scaryAudioSource;
    [Tooltip("AudioSource for Aysor (Aruch only).")]
    public AudioSource audioAysor;
    [Tooltip("AudioSource for Memoir (Hovhanes only).")]
    public AudioSource audioMemoir;

    [Header("Volume Settings")]
    public float maxVolume = 0.3f;
    public float minVolume = 0.0f;
    public float crossfadeSpeed = 1.5f;

    [Header("Pitch Settings (Arousal)")]
    public bool affectPitch = false;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    private float targetGlobalVolume;
    private float targetScaryVolume;
    private float targetPitch = 1.0f;

    void Start() {
        if (globalAudioSource == null)
            globalAudioSource = GetComponent<AudioSource>();

        if (scaryAudioSource == null) {
            var child = new GameObject("ScaryAudioSource");
            child.transform.SetParent(transform);
            scaryAudioSource = child.AddComponent<AudioSource>();
        }
        scaryAudioSource.playOnAwake = true;
        scaryAudioSource.loop = true;
        scaryAudioSource.volume = 0f;

        if (audioAysor == null) {
            var child = new GameObject("AysorAudioSource");
            child.transform.SetParent(transform);
            audioAysor = child.AddComponent<AudioSource>();
            audioAysor.playOnAwake = true;
            audioAysor.loop = true;
            audioAysor.volume = 0f;
        }

        if (audioMemoir == null) {
            var child = new GameObject("MemoirAudioSource");
            child.transform.SetParent(transform);
            audioMemoir = child.AddComponent<AudioSource>();
            audioMemoir.playOnAwake = true;
            audioMemoir.loop = true;
            audioMemoir.volume = 0f;
        }

        targetGlobalVolume = globalAudioSource.volume;
        targetScaryVolume = scaryAudioSource.volume;

        if (AffectiveManager.Instance != null) {
            AffectiveManager.Instance.OnEmotionDataReceived.AddListener(UpdateAudioParameters);
        }
    }

    private void UpdateAudioParameters(TcpSocketClient.EmotionData data) {
        if (!AffectiveManager.IsAffectiveSceneActive) return;

        // Valence maps to crossfade
        float valenceNormalized = Mathf.Clamp01((data.smoothed_valence + 1f) / 2f);
        targetGlobalVolume = Mathf.Lerp(minVolume, maxVolume, valenceNormalized);
        targetScaryVolume = Mathf.Lerp(maxVolume, minVolume, valenceNormalized);

        // Arousal maps to pitch (optional)
        if (affectPitch) {
            float arousalNormalized = Mathf.Clamp01((data.smoothed_arousal + 1f) / 2f);
            targetPitch = Mathf.Lerp(minPitch, maxPitch, arousalNormalized);
        }
    }

    void Update() {
        string templeName = AffectiveManager.currentTempleName != null ? AffectiveManager.currentTempleName.ToLower() : "";
        bool inTemple = AffectiveManager.IsAffectiveSceneActive && !string.IsNullOrEmpty(AffectiveManager.currentTempleName);

        // Global plays when outside any temple
        float currentTargetGlobal = (!inTemple) ? targetGlobalVolume : 0f;

        float currentTargetScary = 0f;
        float currentTargetAysor = 0f;
        float currentTargetMemoir = 0f;

        if (inTemple) {
            if (templeName.Contains("hovhannes")) {
                currentTargetScary = targetScaryVolume;
                currentTargetMemoir = maxVolume;
            } else if (templeName.Contains("aruch")) {
                currentTargetAysor = maxVolume;
            }
        }

        // Smoothly interpolate volumes
        globalAudioSource.volume = Mathf.Lerp(globalAudioSource.volume, currentTargetGlobal, Time.deltaTime * crossfadeSpeed);
        scaryAudioSource.volume = Mathf.Lerp(scaryAudioSource.volume, currentTargetScary, Time.deltaTime * crossfadeSpeed);
        audioAysor.volume = Mathf.Lerp(audioAysor.volume, currentTargetAysor, Time.deltaTime * crossfadeSpeed);
        audioMemoir.volume = Mathf.Lerp(audioMemoir.volume, currentTargetMemoir, Time.deltaTime * crossfadeSpeed);

        if (affectPitch) {
            globalAudioSource.pitch = Mathf.Lerp(globalAudioSource.pitch, targetPitch, Time.deltaTime * crossfadeSpeed);
            scaryAudioSource.pitch = Mathf.Lerp(scaryAudioSource.pitch, targetPitch, Time.deltaTime * crossfadeSpeed);
            audioAysor.pitch = Mathf.Lerp(audioAysor.pitch, targetPitch, Time.deltaTime * crossfadeSpeed);
            audioMemoir.pitch = Mathf.Lerp(audioMemoir.pitch, targetPitch, Time.deltaTime * crossfadeSpeed);
        }
    }

    private void OnDestroy() {
        if (AffectiveManager.Instance != null) {
            AffectiveManager.Instance.OnEmotionDataReceived.RemoveListener(UpdateAudioParameters);
        }
    }
}
