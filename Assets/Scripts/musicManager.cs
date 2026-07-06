using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip ambientMusic;
    public AudioClip specialMusic;

    private void Start()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        PlayAmbient();
    }

    public void PlayAmbient()
    {
        if (audioSource != null && ambientMusic != null)
        {
            if (audioSource.clip != ambientMusic)
                audioSource.clip = ambientMusic;

            audioSource.volume = 0.06f;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void PlaySpecial()
    {
        if (audioSource != null && specialMusic != null)
        {
            if (audioSource.clip != specialMusic)
                audioSource.clip = specialMusic;

            audioSource.volume = 0.1f;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StopAll()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void ResumeAmbient()
    {
        if (audioSource != null && ambientMusic != null)
        {
            if (audioSource.clip != ambientMusic)
                audioSource.clip = ambientMusic;

            audioSource.volume = 0.06f;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
}
