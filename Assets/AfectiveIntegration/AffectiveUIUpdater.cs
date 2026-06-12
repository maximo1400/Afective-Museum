using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class AffectiveUIUpdater : MonoBehaviour
{
    public TMP_Text valenceText;
    public TMP_Text arousalText;
    public GameObject brainImageObject;

    private MockAffectiveData mockData;
    private TcpSocketClient tcpClient;

    private void Start()
    {
        // Find the Mock script and TCP client in the scene
        mockData = FindFirstObjectByType<MockAffectiveData>();
        tcpClient = FindFirstObjectByType<TcpSocketClient>();
    }

    private void Update()
    {
        // Determine if we are connected to the actual python server
        bool isConnected = tcpClient != null && tcpClient.IsConnected;

        if (!AffectiveManager.IsAffectiveSceneActive) isConnected = false;

        // Toggle the brain image visibility based on connection status
        if (brainImageObject != null && brainImageObject.activeSelf != isConnected)
        {
            brainImageObject.SetActive(isConnected);
        }
    }

    // This matches the FloatEvent signature in AffectiveManager
    public void UpdateValenceDisplay(float valenceValue)
    {
        // valenceText.text = $"Valence: {valenceValue:F2}";
    }

    public void UpdateArousalDisplay(float arousalValue)
    {
        // arousalText.text = $"Arousal: {arousalValue:F2}";
    }

    public void ShowLowValenceWarning()
    {
        // Debug.Log("Low Valence Detected!");
    }

    public void ShowHighArousalWarning()
    {
        // Debug.Log("High Arousal Detected!");
    }
}