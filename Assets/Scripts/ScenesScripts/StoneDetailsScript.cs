/*
    StoneDetailsScript.cs
    
    @author Gabriel Azócar Cárcamo <azocarcarcamo@gmail.com>
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Xml;
using UnityEngine.InputSystem;

public class StoneDetailsScript : MonoBehaviour
{
    [SerializeField]
    public List<Text> metaText;
    public GameObject scrollView;
    public GameObject mainCamera;
    public GameObject stone;
    public GameObject loadScreen;
    public Slider slider;
    private StoneService stoneService;
    // Use this for initialization
    void Start()
    {
        stoneService = gameObject.AddComponent<StoneService>();
        stoneService.loadScreen = this.loadScreen;
        stoneService.slider = this.slider;

        string[] firstSplit = StaticValues.stone_name.Split('(');
        string number = firstSplit[0].Replace("Stone", "");
        try
        {
            int sID = Int32.Parse(number);
            SpawnStone(sID);
        }
        catch (FormatException)
        {
            Debug.Log("Start Details Error");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.stone != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll != 0.0f)
            {
                float trans = scroll < 0 ? 0.9f : 1.1f;
                this.stone.transform.localScale *= trans;
            }

            if (Mouse.current.leftButton.isPressed)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                this.stone.transform.Rotate(Vector3.up, 0.5f * delta.x, Space.World);
                this.stone.transform.Rotate(Vector3.right, -0.5f * delta.y, Space.World);
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                // Snap block on x axis 
                Vector3 euler = this.stone.transform.eulerAngles;
                euler.x = Mathf.Round(euler.x / 45f) * 45f;
                this.stone.transform.rotation = Quaternion.Euler(euler);
            }

            if (Mouse.current.rightButton.isPressed)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                this.stone.transform.position += new Vector3(delta.x * 0.01f, delta.y * 0.01f, 0);
            }
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(StaticValues.previos_scene, LoadSceneMode.Single);
        }
    }

    public void SpawnStone(int stoneId)
    {
        Vector3 pos = this.stone.transform.position;
        StartCoroutine(this.stoneService.SpawnStoneWithPositionAndRotation(stoneId, pos, FinishConfig, true));
    }

    public void FinishConfig()
    {
        FindStoneObject(StaticValues.stone_name);
        loadJSON(StaticValues.stone_name);
    }

    IEnumerator loadJSON(string stoneName)
    {
        if (stoneName.Contains("Clone"))
        {
            stoneName = stoneName.Split('(')[0];
        }

        try
        {
            int index = Int32.Parse(stoneName.Replace("Stone", ""));
            StoneService.BundleName bundleName = StoneService.CalculateAssetBundleNameByStoneIndex(index);

            TextAsset metadata = null;
            foreach (AssetBundle mab in StonesValues.metadataAssetBundles)
            {
                if (mab.name == bundleName.metadataBundleName)
                {
                    metadata = mab.LoadAsset<TextAsset>(stoneName);
                }
            }
            Khachkar khachkar = JsonUtility.FromJson<Khachkar>(metadata.text);
            metaText[0].text = khachkar.conditionOfPreservation;
            metaText[1].text = khachkar.importantFeatures;
            metaText[2].text = khachkar.location;
            metaText[4].text = "Accessibility: " + khachkar.accessibility;
            metaText[6].text = "Production Period: " + khachkar.productionPeriod;

        }
        catch (FormatException)
        {
            Debug.Log("Error loading metadata");
        }

        return null;
    }

    private string FormatMetaText(string metaText)
    {
        if (metaText != null && metaText != "")
        {
            if (metaText[metaText.Length - 1] != '.')
            {
                return metaText.Trim() + ".";
            }
            return metaText.Trim();
        }

        return "No data.";
    }

    private void FindStoneObject(string stoneName)
    {
        foreach (GameObject obj in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj.name.Contains(stoneName))
            {
                this.stone = obj;
            }
        }
    }
}
