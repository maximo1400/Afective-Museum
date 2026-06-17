using UnityEngine;

public class AffectiveTempleTrigger : MonoBehaviour {
    [Tooltip("Identifier for this temple")]
    [SerializeField] private string templeName;
    private int collidersInside = 0;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            collidersInside++;
            if (collidersInside == 1) {
                AffectiveManager.currentTempleName = templeName;
                Debug.Log($"[Affective] Entered temple: {templeName}");
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            collidersInside--;
            if (collidersInside <= 0) {
                collidersInside = 0;
                AffectiveManager.currentTempleName = "";
                Debug.Log($"[Affective] Exited temple: {templeName}");
            }
        }
    }
}
