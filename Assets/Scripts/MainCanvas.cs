using UnityEngine.InputSystem;
﻿using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    public GameObject helpPane;

    // Use this for initialization
    void Start()
    {
        helpPane.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {

            helpPane.SetActive(!helpPane.activeSelf);
        }
    }
}
