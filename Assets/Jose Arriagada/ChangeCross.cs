using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CubeModelChanger : MonoBehaviour
{
    public GameObject[] modelos;
    public Camera cameraUtil;
    public FirstPersonController fpc;
    public GameObject interactionCanvas;
    public Collider floorCollider;
    private int currentModelIndex = 0;
    private static bool isCameraActive = false;
    private Camera mainCamera;
    private bool isPlayerInRange = false;

    void Start()
    {
        mainCamera = Camera.main;

        if (cameraUtil != null)
            cameraUtil.gameObject.SetActive(false);

        if (fpc == null)
        {
            fpc = FindFirstObjectByType<FirstPersonController>();
            if (fpc == null)
                Debug.LogWarning("No se encontró un FirstPersonController en la escena.");
        }

        if (interactionCanvas != null)
            interactionCanvas.SetActive(false);

        // Activar solo el primer modelo
        for (int i = 0; i < modelos.Length; i++)
            modelos[i].SetActive(i == currentModelIndex);
    }

    void Update()
    {
        if (isPlayerInRange && !isCameraActive && Keyboard.current.qKey.wasPressedThisFrame)
        {
            ActivateCamera();
        }
    }

    void OnMouseDown()
    {
        if (!isCameraActive)
            return;

        // Desactivar modelo actual
        modelos[currentModelIndex].SetActive(false);

        // Cambiar índice
        currentModelIndex = (currentModelIndex + 1) % modelos.Length;

        // Activar nuevo modelo
        modelos[currentModelIndex].SetActive(true);
    }

    public void ActivateCamera()
    {
        if (cameraUtil != null && mainCamera != null)
        {
            cameraUtil.gameObject.SetActive(true);
            mainCamera.gameObject.SetActive(false);
            isCameraActive = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (fpc != null)
                fpc.enabled = false;

            if (interactionCanvas != null)
                interactionCanvas.SetActive(false);

            // Desactivar el collider del piso
            if (floorCollider != null)
                floorCollider.enabled = false;
        }
    }


    public void ResetToMainCamera()
    {
        if (cameraUtil != null && mainCamera != null)
        {
            cameraUtil.gameObject.SetActive(false);
            mainCamera.gameObject.SetActive(true);
            isCameraActive = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (fpc != null)
                fpc.enabled = true;

            // 🟢 Reactivar el collider del piso
            if (floorCollider != null)
                floorCollider.enabled = true;
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
