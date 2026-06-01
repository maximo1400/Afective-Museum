using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.InputSystem;

public class CameraActivator : MonoBehaviour
{
    public Camera interactionCamera;
    public GameObject interactionCanvas;

    private FirstPersonController playerController;
    private bool isPlayerInRange = false;
    private static bool isCameraActive = false;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (interactionCamera != null)
            interactionCamera.gameObject.SetActive(false);

        if (interactionCanvas != null)
            interactionCanvas.SetActive(false);

        // 🔍 Detección automática del jugador usando FindObjectOfType
        if (playerController == null)
        {
            playerController = FindObjectOfType<FirstPersonController>();
            if (playerController == null)
                Debug.LogWarning("No se encontró un FirstPersonController en la escena.");
        }
    }

    void Update()
    {
        if (isPlayerInRange && !isCameraActive && Keyboard.current.qKey.wasPressedThisFrame)
        {
            ActivateCamera();
        }
        else if (isCameraActive && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ResetToMainCamera();
        }
    }

    public void ActivateCamera()
    {
        if (interactionCamera != null && mainCamera != null)
        {
            interactionCamera.gameObject.SetActive(true);
            mainCamera.gameObject.SetActive(false);
            isCameraActive = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerController != null)
                playerController.enabled = false;

            if (interactionCanvas != null)
                interactionCanvas.SetActive(false);
        }
    }

    public void ResetToMainCamera()
    {
        if (interactionCamera != null && mainCamera != null)
        {
            interactionCamera.gameObject.SetActive(false);
            mainCamera.gameObject.SetActive(true);
            isCameraActive = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerController != null)
                playerController.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactionCanvas != null)
                interactionCanvas.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactionCanvas != null)
                interactionCanvas.SetActive(false);
        }
    }
}
