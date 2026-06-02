using UnityEngine;

public class MusicZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MusicManager music = FindFirstObjectByType<MusicManager>();
            if (music != null)
            {
                music.StopAll(); // Detiene música ambiental
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MusicManager music = FindFirstObjectByType<MusicManager>();
            if (music != null)
            {
                music.PlayAmbient(); // Vuelve la música ambiental al salir
            }
        }
    }
}
