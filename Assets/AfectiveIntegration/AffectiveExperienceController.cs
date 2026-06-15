using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class AffectiveExperienceController : MonoBehaviour {
    [Header("Screenshot Settings")]
    private float highIntensityThreshold = 3f;
    private float screenshotCooldown = 10f;
    private float firstScreenshotTime = 10f;
    private float lastScreenshotTime;

    [Header("Lost Player Settings")]
    private float lostValenceThreshold = 1f;
    private float lostTimeThreshold = 30f;
    private float timeInNegativeValence = 0f;

    // Position tracking to check if player is actually lost/stuck
    private Transform playerTransform;
    [SerializeField] private float movementThreshold;
    private Vector3 lastRecordedPosition;
    private float timeStuck = 0f;
    private string sessionStartTimeStr;
    private float lastPacketTime;
    private float timeSinceLastPacket;

    void Start() {
        lastScreenshotTime = Time.time + firstScreenshotTime; // Delay first screenshot to give player time to settle in
        sessionStartTimeStr = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        lastPacketTime = Time.time;

        if (AffectiveManager.Instance != null) {
            AffectiveManager.Instance.OnEmotionDataReceived.AddListener(CheckExperienceTriggers);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        FindPlayerTransform();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode) {
        FindPlayerTransform();
    }

    private void FindPlayerTransform() {
        // Unity's == null check will evaluate to true if the previous playerTransform was destroyed on scene load
        if (playerTransform == null && Camera.main != null) {
            playerTransform = Camera.main.transform;
        }

        if (playerTransform != null) {
            lastRecordedPosition = playerTransform.position;
            timeStuck = 0f; // Reset stuck time for new scene
        }
    }

    private void CheckExperienceTriggers(TcpSocketClient.EmotionData data) {
        // 1. High Intensity (Arousal) -> Take Screenshot
        if (data.smoothed_arousal >= highIntensityThreshold && Time.time - lastScreenshotTime > screenshotCooldown) {
            TakeIntensityScreenshot(data);
        }

        // 2. Lost/Frustrated State
        CheckLostState(data);
    }

    private void TakeIntensityScreenshot(TcpSocketClient.EmotionData data) {
        if (!AffectiveManager.IsAffectiveSceneActive) return;

        lastScreenshotTime = Time.time;
        string baseFolderPath = Path.Combine(Application.dataPath, "../AffectiveReports/");
        string folderPath = Path.Combine(baseFolderPath, $"Session_{sessionStartTimeStr}");

        if (!Directory.Exists(folderPath)) {
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

    private void CheckLostState(TcpSocketClient.EmotionData data) {
        if (!AffectiveManager.IsAffectiveSceneActive) return;

        if (playerTransform == null) return;

        timeSinceLastPacket = Time.time - lastPacketTime;
        lastPacketTime = Time.time;

        // Check if player has barely moved
        if (Vector3.Distance(playerTransform.position, lastRecordedPosition) < movementThreshold) {
            timeStuck += timeSinceLastPacket;
        } else {
            timeStuck = 0f;
            lastRecordedPosition = playerTransform.position;
        }

        // Check if player is frustrated (negative valence)
        if (data.smoothed_valence <= lostValenceThreshold) {
            timeInNegativeValence += timeSinceLastPacket;
        } else {
            timeInNegativeValence = 0f;
        }

        // Trigger Help
        if (timeStuck >= lostTimeThreshold && timeInNegativeValence >= lostTimeThreshold) {
            ProvideHelp();
            // Reset timers to avoid spamming help
            timeStuck = 0f;
            timeInNegativeValence = 0f;
        }
    }

    private void ProvideHelp() {
        Debug.Log("Player seems lost and frustrated! Providing help...");
        // TODO: Implement actual visual/audio help here
        // e.g., Spawn a guiding fairy, highlight the path, or show UI text.
    }

    private void OnDestroy() {
        if (AffectiveManager.Instance != null) {
            AffectiveManager.Instance.OnEmotionDataReceived.RemoveListener(CheckExperienceTriggers);
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
