using UnityEngine;
using System.IO;
using System.Collections;

public class AffectiveExperienceController : MonoBehaviour
{
    [Header("Screenshot Settings")]
    public float highIntensityThreshold = 0.8f;
    public float screenshotCooldown = 10f;
    private float lastScreenshotTime = -100f;
    
    [Header("Lost Player Settings")]
    public float lostValenceThreshold = 1f;
    public float lostTimeThreshold = 30f;
    private float timeInNegativeValence = 0f;
    
    // Position tracking to check if player is actually lost/stuck
    public Transform playerTransform;
    public float movementThreshold = 2.0f;
    private Vector3 lastRecordedPosition;
    private float timeStuck = 0f;

    void Start()
    {
        if (AffectiveManager.Instance != null)
        {
            AffectiveManager.Instance.OnEmotionDataReceived.AddListener(CheckExperienceTriggers);
        }

        if (playerTransform == null && Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }

        if (playerTransform != null)
        {
            lastRecordedPosition = playerTransform.position;
        }
    }

    private void CheckExperienceTriggers(TcpSocketClient.EmotionData data)
    {
        // 1. High Intensity (Arousal) -> Take Screenshot
        if (data.smoothed_arousal >= highIntensityThreshold && Time.time - lastScreenshotTime > screenshotCooldown)
        {
            TakeIntensityScreenshot(data);
        }

        // 2. Lost/Frustrated State
        CheckLostState(data);
    }

    private void TakeIntensityScreenshot(TcpSocketClient.EmotionData data)
    {
        lastScreenshotTime = Time.time;
        string folderPath = Path.Combine(Application.dataPath, "../AffectiveReports");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string timestampStr = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string screenshotPath = Path.Combine(folderPath, $"Intensity_Screenshot_{timestampStr}.png");
        string reportPath = Path.Combine(folderPath, $"Intensity_Report_{timestampStr}.txt");

        ScreenCapture.CaptureScreenshot(screenshotPath);
        
        string reportContent = $"High Intensity Event Logged at {System.DateTime.Now}\n" +
                               $"Valence: {data.smoothed_valence}\n" +
                               $"Arousal: {data.smoothed_arousal}\n" +
                               $"Confidence: {data.confidence}";
                               
        File.WriteAllText(reportPath, reportContent);
        Debug.Log($"High Intensity Event! Screenshot saved to: {screenshotPath}");
    }

    private void CheckLostState(TcpSocketClient.EmotionData data)
    {
        if (playerTransform == null) return;

        // Check if player has barely moved
        if (Vector3.Distance(playerTransform.position, lastRecordedPosition) < movementThreshold)
        {
            timeStuck += Time.deltaTime;
        }
        else
        {
            timeStuck = 0f;
            lastRecordedPosition = playerTransform.position;
        }

        // Check if player is frustrated (negative valence)
        if (data.smoothed_valence <= lostValenceThreshold)
        {
            timeInNegativeValence += Time.deltaTime;
        }
        else
        {
            timeInNegativeValence = 0f;
        }

        // Trigger Help
        if (timeStuck >= lostTimeThreshold && timeInNegativeValence >= lostTimeThreshold)
        {
            ProvideHelp();
            // Reset timers to avoid spamming help
            timeStuck = 0f;
            timeInNegativeValence = 0f;
        }
    }

    private void ProvideHelp()
    {
        Debug.Log("Player seems lost and frustrated! Providing help...");
        // TODO: Implement actual visual/audio help here
        // e.g., Spawn a guiding fairy, highlight the path, or show UI text.
    }

    private void OnDestroy()
    {
        if (AffectiveManager.Instance != null)
        {
            AffectiveManager.Instance.OnEmotionDataReceived.RemoveListener(CheckExperienceTriggers);
        }
    }
}
