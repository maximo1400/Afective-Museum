using UnityEngine;

public class PersistentObject : MonoBehaviour {
    void Awake() {
        // This tells Unity not to destroy this GameObject when a new scene is loaded.
        // As a result, the TCP connection will stay alive across different rooms/scenes!
        DontDestroyOnLoad(this.gameObject);
    }
}
