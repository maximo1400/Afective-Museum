using UnityEngine;
using UnityEngine.InputSystem;

public class Stone : MonoBehaviour
{
    public static int collectedStones = 0;
    public static int totalStones = 5;

    private bool isPlayerNearby = false;

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.qKey.wasPressedThisFrame)
        {
            collectedStones++;
            Debug.Log($"[STONE] Stone Collected. Total: {collectedStones}/{totalStones}");
            gameObject.SetActive(false);
            if (collectedStones >= totalStones)
            {
                Debug.Log("🔓 The door has open");
            }
        }

    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[STONE] Trigger ENTER con: {other.name}");

        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("[STONE] Jugador cerca. Presiona Q para recoger.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[STONE] Trigger EXIT con: {other.name}");

        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

   /*void OnMouseDown()
    {
        if (true)
        {
            collectedStones++;
            Debug.Log($"[STONE] Stone Collected. Total: {collectedStones}/{totalStones}");
            gameObject.SetActive(false);
            if (collectedStones >= totalStones)
            {
                Debug.Log("🔓 The door has open");
            }
        }
        pass;
    }*/
}

