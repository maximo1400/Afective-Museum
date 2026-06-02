using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.InputSystem;

public class GamificationHelper : MonoBehaviour
{
    public Texture[] textures;
    public Camera cameraUtil;
    public Canvas interactionCanvas;
    public FirstPersonController fpc;

    private Renderer cubeRenderer;
    private int currentTextureIndex = 0;
    private static bool isCameraActive = false;
    private Camera mainCamera;
    private bool isPlayerNearby = false;

    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();

        if (textures.Length > 0)
            cubeRenderer.material.mainTexture = textures[currentTextureIndex];

        mainCamera = Camera.main;

        if (fpc == null)
        {
            fpc = FindFirstObjectByType<FirstPersonController>();
            if (fpc == null)
            {
                Debug.LogWarning("No se encontró un FirstPersonController en la escena.");
            }
        }
        
        if (cameraUtil != null)
            cameraUtil.gameObject.SetActive(false);

        if (interactionCanvas != null)
            interactionCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isPlayerNearby && Keyboard.current.qKey.wasPressedThisFrame)
        {
            ActivateCameraAndDisableFPC();
        }
    }

    private void ActivateCameraAndDisableFPC()
    {
        if (!isCameraActive)
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
            }

            if (interactionCanvas != null)
                interactionCanvas.gameObject.SetActive(false);
        }
        else
        {
            currentTextureIndex = (currentTextureIndex + 1) % textures.Length;
            cubeRenderer.material.mainTexture = textures[currentTextureIndex];
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            if (interactionCanvas != null)
                interactionCanvas.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            if (interactionCanvas != null)
                interactionCanvas.gameObject.SetActive(false);
        }
    }

}
