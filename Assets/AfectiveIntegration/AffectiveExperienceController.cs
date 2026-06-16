using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
    public bool enableHelpSystem = true;
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
    private GameObject currentHelpArrow;

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
        if (!AffectiveManager.IsAffectiveSceneActive || !enableHelpSystem) return;

        Debug.Log("Player seems lost and frustrated! Providing help...");
        ShowClosestStoneHelp();
    }

    private void ShowClosestStoneHelp() {
        if (playerTransform == null) return;
        if (currentHelpArrow != null) return;

        Stone[] stones = FindObjectsByType<Stone>(FindObjectsSortMode.None);
        if (stones.Length == 0) return;

        Stone closestStone = null;
        float minDistance = float.MaxValue;
        foreach (Stone stone in stones) {
            if (!stone.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(playerTransform.position, stone.transform.position);
            if (dist < minDistance) {
                minDistance = dist;
                closestStone = stone;
            }
        }
        if (closestStone != null) {
            StartCoroutine(SpawnHelpColumn(closestStone.transform.position, closestStone));
        }
    }

    private IEnumerator SpawnHelpColumn(Vector3 position, Stone targetStone) {
        GameObject arrowParent = new GameObject("HelpArrow");
        currentHelpArrow = arrowParent;
        // Start higher up so the tail doesn't clip into the floor
        arrowParent.transform.position = position + Vector3.up * 15f;

        // 1. Create Tail (Cylinder)
        GameObject tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(tail.GetComponent<Collider>());
        tail.transform.SetParent(arrowParent.transform);

        float tailLength = 60f;
        tail.transform.localScale = new Vector3(1.5f, tailLength, 1.5f);
        // Primitive cylinder is 2 units tall, so height is 120. Center it so the bottom is at Y=0.
        tail.transform.localPosition = new Vector3(0, tailLength, 0);

        // 2. Create Head (Cone)
        GameObject head = CreateCone();
        head.transform.SetParent(arrowParent.transform);
        // Scale the cone to look like a thick arrowhead
        head.transform.localScale = new Vector3(5f, 6f, 5f);
        // Rotate it to point downwards
        head.transform.localRotation = Quaternion.Euler(180f, 0f, 0f);
        head.transform.localPosition = Vector3.zero; // Attach to the bottom of the tail

        // 3. Setup Material
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.8f, 0f, 0.6f);

        // Transparent settings
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;

        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0f) * 1.5f);

        tail.GetComponent<Renderer>().material = mat;
        head.GetComponent<Renderer>().material = mat;

        // --- Create 2D UI Compass ---
        GameObject canvasGO = new GameObject("CompassCanvas");
        canvasGO.transform.SetParent(arrowParent.transform);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        canvas.sortingOrder = 100;

        GameObject textGO = new GameObject("CompassText");
        textGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI uiCompassText = textGO.AddComponent<TextMeshProUGUI>();
        uiCompassText.text = "↑";
        uiCompassText.fontSize = 120;
        uiCompassText.color = new Color(1f, 0.8f, 0f, 0.9f);
        uiCompassText.alignment = TextAlignmentOptions.Center;
        uiCompassText.overflowMode = TextOverflowModes.Overflow;

        RectTransform rt = uiCompassText.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 200);
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, -150); // 150 pixels from top
        // ----------------------

        // 4. Keep visible until stone is collected
        while (true) {
            if (targetStone == null || !targetStone.gameObject.activeInHierarchy) {
                break;
            }

            if (playerTransform != null && uiCompassText != null) {
                Vector3 directionToStone = targetStone.transform.position - playerTransform.position;
                directionToStone.y = 0;

                Vector3 playerForward = playerTransform.forward;
                playerForward.y = 0;

                if (directionToStone.sqrMagnitude > 0.001f && playerForward.sqrMagnitude > 0.001f) {
                    float angle = Vector3.SignedAngle(playerForward.normalized, directionToStone.normalized, Vector3.up);
                    // Rotate the UI Text around Z axis
                    rt.localRotation = Quaternion.Euler(0, 0, -angle);
                }
            }

            yield return null;
        }

        if (currentHelpArrow == arrowParent) {
            currentHelpArrow = null;
        }
        Destroy(arrowParent);
    }

    private GameObject CreateCone() {
        GameObject go = new GameObject("Cone");
        MeshFilter filter = go.AddComponent<MeshFilter>();
        MeshRenderer renderer = go.AddComponent<MeshRenderer>();
        Mesh mesh = new Mesh();

        int segments = 18;
        float radius = 0.5f;
        float height = 1f;

        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 6];

        vertices[0] = new Vector3(0, height, 0); // Tip
        vertices[1] = new Vector3(0, 0, 0);      // Base center

        for (int i = 0; i < segments; i++) {
            float angle = (float)i / segments * Mathf.PI * 2f;
            vertices[i + 2] = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        }

        for (int i = 0; i < segments; i++) {
            int current = i + 2;
            int next = (i + 1) % segments + 2;

            // Side
            triangles[i * 6] = 0;
            triangles[i * 6 + 1] = current;
            triangles[i * 6 + 2] = next;

            // Base
            triangles[i * 6 + 3] = 1;
            triangles[i * 6 + 4] = next;
            triangles[i * 6 + 5] = current;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        filter.mesh = mesh;
        return go;
    }

    private void OnDestroy() {
        if (AffectiveManager.Instance != null) {
            AffectiveManager.Instance.OnEmotionDataReceived.RemoveListener(CheckExperienceTriggers);
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
