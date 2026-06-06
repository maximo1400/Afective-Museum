using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeactivateOnZ : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current.zKey.wasPressedThisFrame)
        {
            gameObject.SetActive(false);
        }
    }
}
