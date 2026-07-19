using UnityEngine;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class AffectiveAudioController : MonoBehaviour {
    [Header("Audio Sources")]
    [Tooltip("The main AudioSource on this object, used for global/positive valence music.")]
    public AudioSource globalAudioSource;
    [Tooltip("A secondary AudioSource, used for Storm/negative valence music (Hovhanes only).")]
    public AudioSource stormAudioSource;
    [Tooltip("AudioSource for Aysor (Aruch only).")]
    public AudioSource audioAysor;

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
    private float targetPitch = 1.0f;
    private float globalVolume;
    private double unityStartingTimestamp;
    private string sessionStartTimeStr;

    void Start() {
        unityStartingTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        sessionStartTimeStr = ((long)unityStartingTimestamp).ToString();

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

        globalVolume = globalAudioSource.volume;
        targetGlobalVolume = globalVolume;
        targetStormVolume = stormAudioSource.volume;
        targetAysorVolume = audioAysor.volume;

        if (AffectiveManager.Instance != null) {
            AffectiveManager.Instance.OnEmotionDataReceived.AddListener(UpdateAudioParameters);
        }
    }

    private void UpdateAudioParameters(TcpSocketClient.EmotionData data) {
        if (!AffectiveManager.IsAffectiveSceneActive) {
            targetGlobalVolume = globalVolume;
            targetStormVolume = 0f;
            targetAysorVolume = 0f;
            return;
        }

        string templeName = AffectiveManager.currentTempleName;
        float valenceNormalized = Mathf.InverseLerp(-1f, 1f, data.valence);
        float arousalNormalized = Mathf.InverseLerp(-1f, 1f, data.arousal);

        targetStormVolume = 0f;
        targetAysorVolume = 0f;
        targetGlobalVolume = 0f;

        if (templeName == "Hovhannes") {
            targetStormVolume = Mathf.Lerp(minVolume, maxVolume, arousalNormalized);

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

        LogAudioData(data);
    }

    private void LogAudioData(TcpSocketClient.EmotionData data) {
        if (!AffectiveManager.IsAffectiveSceneActive) return;

        string baseFolderPath = Path.Combine(Application.dataPath, "../AffectiveReports/");
        string folderPath = Path.Combine(baseFolderPath, $"Session_{sessionStartTimeStr}");

        if (!Directory.Exists(folderPath)) {
            Directory.CreateDirectory(folderPath);
        }

        string reportPath = Path.Combine(folderPath, $"audio_out_{sessionStartTimeStr}.csv");
        bool writeHeader = !File.Exists(reportPath);

        using StreamWriter writer = new(reportPath, true);
        if (writeHeader) {
            writer.WriteLine("timestamp,temple,valence,arousal,confidence,data_timestamp,unity_timestamp,data_starting_timestamp,unity_starting_timestamp,global_volume,storm_volume,aysor_volume,pitch");
        }
        double currentUnityTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        string templeName = string.IsNullOrEmpty(AffectiveManager.currentTempleName) ? "None" : AffectiveManager.currentTempleName;
        string row = $"{System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss},{templeName},{data.valence},{data.arousal},{data.confidence},{data.timestamp:F10},{currentUnityTimestamp:F10},{data.starting_timestamp:F10},{unityStartingTimestamp:F10},{targetGlobalVolume},{targetStormVolume},{targetAysorVolume},{targetPitch}";
        writer.WriteLine(row);
    }

    void Update() {
        // Smoothly interpolate volumes directly to their targets calculated upon receiving emotion data
        globalAudioSource.volume = Mathf.Lerp(globalAudioSource.volume, targetGlobalVolume, Time.deltaTime * crossfadeSpeed);
        stormAudioSource.volume = Mathf.Lerp(stormAudioSource.volume, targetStormVolume, Time.deltaTime * crossfadeSpeed);
        audioAysor.volume = Mathf.Lerp(audioAysor.volume, targetAysorVolume, Time.deltaTime * crossfadeSpeed);

        if (affectPitch) {
            globalAudioSource.pitch = Mathf.Lerp(globalAudioSource.pitch, targetPitch, Time.deltaTime * crossfadeSpeed);
            stormAudioSource.pitch = Mathf.Lerp(stormAudioSource.pitch, targetPitch, Time.deltaTime * crossfadeSpeed);
            audioAysor.pitch = Mathf.Lerp(audioAysor.pitch, targetPitch, Time.deltaTime * crossfadeSpeed);
        }
    }

    private void OnDestroy() {
        if (AffectiveManager.Instance != null) {
            AffectiveManager.Instance.OnEmotionDataReceived.RemoveListener(UpdateAudioParameters);
        }
    }
}
