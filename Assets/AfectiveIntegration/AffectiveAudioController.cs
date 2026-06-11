using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AffectiveAudioController : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("The main AudioSource on this object, used for relaxing/positive valence music.")]
    public AudioSource relaxingAudioSource;
    [Tooltip("A secondary AudioSource on a child object, used for scary/negative valence music.")]
    public AudioSource scaryAudioSource;

    [Header("Volume Settings")]
    public float maxVolume = 1.0f;
    public float minVolume = 0.0f;
    public float crossfadeSpeed = 1.5f;

    [Header("Pitch Settings (Arousal)")]
    public bool affectPitch = false;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    private float targetRelaxingVolume;
    private float targetScaryVolume;
    private float targetPitch = 1.0f;

    void Start()
    {
        if (relaxingAudioSource == null)
            relaxingAudioSource = GetComponent<AudioSource>();

        if (scaryAudioSource == null)
        {
            // Try to find a child audio source or create one
            var child = new GameObject("ScaryAudioSource");
            child.transform.SetParent(transform);
            scaryAudioSource = child.AddComponent<AudioSource>();
            scaryAudioSource.loop = true;
            scaryAudioSource.playOnAwake = true;
            // The user will need to assign the scary audio clip in the inspector
        }

        targetRelaxingVolume = relaxingAudioSource.volume;
        targetScaryVolume = scaryAudioSource.volume;

        if (AffectiveManager.Instance != null)
        {
            AffectiveManager.Instance.OnEmotionDataReceived.AddListener(UpdateAudioParameters);
        }
    }

    private void UpdateAudioParameters(TcpSocketClient.EmotionData data)
    {
        // Valence maps to crossfade
        // If valence > 0 (positive), relaxing volume goes up, scary goes down.
        // If valence < 0 (negative), scary volume goes up, relaxing goes down.
        float valenceNormalized = Mathf.Clamp01((data.smoothed_valence + 1f) / 2f);
        
        targetRelaxingVolume = Mathf.Lerp(minVolume, maxVolume, valenceNormalized);
        targetScaryVolume = Mathf.Lerp(maxVolume, minVolume, valenceNormalized);

        // Arousal maps to pitch (optional)
        if (affectPitch)
        {
            float arousalNormalized = Mathf.Clamp01((data.smoothed_arousal + 1f) / 2f);
            targetPitch = Mathf.Lerp(minPitch, maxPitch, arousalNormalized);
        }
    }

    void Update()
    {
        // Smoothly interpolate volumes
        relaxingAudioSource.volume = Mathf.Lerp(relaxingAudioSource.volume, targetRelaxingVolume, Time.deltaTime * crossfadeSpeed);
        scaryAudioSource.volume = Mathf.Lerp(scaryAudioSource.volume, targetScaryVolume, Time.deltaTime * crossfadeSpeed);

        if (affectPitch)
        {
            relaxingAudioSource.pitch = Mathf.Lerp(relaxingAudioSource.pitch, targetPitch, Time.deltaTime * crossfadeSpeed);
            scaryAudioSource.pitch = Mathf.Lerp(scaryAudioSource.pitch, targetPitch, Time.deltaTime * crossfadeSpeed);
        }
    }

    private void OnDestroy()
    {
        if (AffectiveManager.Instance != null)
        {
            AffectiveManager.Instance.OnEmotionDataReceived.RemoveListener(UpdateAudioParameters);
        }
    }
}
