using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HudManager : MonoBehaviour
{
    public static HudManager Instance;

    public string currentSceneName = "";
    public TextMeshProUGUI sceneNameLabel; // ¡Nueva referencia al label!

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
