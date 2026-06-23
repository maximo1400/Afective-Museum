using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AffectiveAudioController : MonoBehaviour {
    [Header("Audio Sources")]
    [Tooltip("The main AudioSource on this object, used for global/positive valence music.")]
    public AudioSource globalAudioSource;
    [Tooltip("A secondary AudioSource, used for Storm/negative valence music (Hovhanes only).")]
    public AudioSource stormAudioSource;
    [Tooltip("AudioSource for Aysor (Aruch only).")]
    public AudioSource audioAysor;
    [Tooltip("AudioSource for Memoir (Hovhanes only).")]
    public AudioSource audioMemoir;

    [Header("Volume Settings")]
    [SerializeField] private float maxVolume = 0.3f;
    [SerializeField] private float minVolume = 0.0f;
    [SerializeField] private float crossfadeSpeed = 1.5f;

    [Header("Pitch Settings (Arousal)")]
    [SerializeField] private bool affectPitch = false;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.2f;

    private float targetGlobalVolume;
    private float targetStormVolume;
    private float targetAysorVolume;
    private float targetMemoirVolume;
    private float targetPitch = 1.0f;
    private float globalVolume;

    void Start() {
        if (globalAudioSource == null)
            globalAudioSource = GetComponent<AudioSource>();


        if (stormAudioSource == null) {
            var child = new GameObject("StormAudioSource");
            child.transform.SetParent(transform);
            stormAudioSource = child.AddComponent<AudioSource>();
            stormAudioSource.playOnAwake = true;
            stormAudioSource.loop = true;
            stormAudioSource.volume = 0f;
        }

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
        globalVolume = globalAudioSource.volume;
        targetGlobalVolume = globalVolume;
        targetStormVolume = stormAudioSource.volume;
        targetAysorVolume = audioAysor.volume;
        targetMemoirVolume = audioMemoir.volume;

        if (AffectiveManager.Instance != null) {
            AffectiveManager.Instance.OnEmotionDataReceived.AddListener(UpdateAudioParameters);
        }
    }

    private void UpdateAudioParameters(TcpSocketClient.EmotionData data) {
        if (!AffectiveManager.IsAffectiveSceneActive) {
            targetGlobalVolume = globalVolume;
            targetStormVolume = 0f;
            targetAysorVolume = 0f;
            targetMemoirVolume = 0f;
            return;
        }

        string templeName = AffectiveManager.currentTempleName;
        float valenceNormalized = Mathf.InverseLerp(-1f, 1f, data.smoothed_valence);
        float arousalNormalized = Mathf.InverseLerp(-1f, 1f, data.smoothed_arousal);

        targetStormVolume = 0f;
        targetAysorVolume = 0f;
        targetMemoirVolume = 0f;
        targetGlobalVolume = 0f;

        if (templeName == "Hovhannes") {
            targetStormVolume = Mathf.Lerp(minVolume, maxVolume, arousalNormalized);
            targetMemoirVolume = Mathf.Lerp(maxVolume, minVolume, arousalNormalized);

        } else if (templeName == "Aruch") {
            targetAysorVolume = Mathf.Lerp(minVolume, maxVolume, valenceNormalized);

        } else {
            // Outside temple
            targetGlobalVolume = Mathf.Lerp(minVolume, globalVolume, valenceNormalized);
        }


        // Arousal maps to pitch (optional)
        if (affectPitch) {
            targetPitch = Mathf.Lerp(minPitch, maxPitch, arousalNormalized);
        }
    }

    void Update() {
        // Smoothly interpolate volumes directly to their targets calculated upon receiving emotion data
        globalAudioSource.volume = Mathf.Lerp(globalAudioSource.volume, targetGlobalVolume, Time.deltaTime * crossfadeSpeed);
        stormAudioSource.volume = Mathf.Lerp(stormAudioSource.volume, targetStormVolume, Time.deltaTime * crossfadeSpeed);
        audioAysor.volume = Mathf.Lerp(audioAysor.volume, targetAysorVolume, Time.deltaTime * crossfadeSpeed);
        audioMemoir.volume = Mathf.Lerp(audioMemoir.volume, targetMemoirVolume, Time.deltaTime * crossfadeSpeed);

        if (affectPitch) {
            globalAudioSource.pitch = Mathf.Lerp(globalAudioSource.pitch, targetPitch, Time.deltaTime * crossfadeSpeed);
            stormAudioSource.pitch = Mathf.Lerp(stormAudioSource.pitch, targetPitch, Time.deltaTime * crossfadeSpeed);
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
