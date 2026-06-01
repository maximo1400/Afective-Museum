using UnityEngine;
using UnityEngine.SceneManagement;  // Necesario para cambiar de escena
using UnityEngine.InputSystem;

public class UsualDoor : MonoBehaviour
{
    private bool isPlayerNearby = false;

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.qKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene("jose"); // Reemplaza con el nombre real de la escena
            }
           
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("entraste");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
