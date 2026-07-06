/*
    StoneService.cs
    
    @author Gabriel Azócar Cárcamo <azocarcarcamo@gmail.com>
 */

using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class StoneService : MonoBehaviour
{
    public struct BundleName
    {
        public BundleName(string stoneBundleName, string metadataBundleName)
        {
            this.stoneBundleName = stoneBundleName;
            this.metadataBundleName = metadataBundleName;
            this.thumbsBundleName = null;
        }

        public BundleName(string thumbsBundleName)
        {
            this.stoneBundleName = null;
            this.metadataBundleName = null;
            this.thumbsBundleName = thumbsBundleName;
        }

        public string stoneBundleName { get; }
        public string metadataBundleName { get; }
        public string thumbsBundleName { get; }
    }

    public GameObject loadScreen;
    public Slider slider;

    public string thumbsBundleName = "stones_thumbs";
    private const string domain = "https://saduewa.dcc.uchile.cl/StreamingAssets/";

    // Use this for initialization
    void Start()
    {

    }

    public IEnumerator DownloadThumbs(Action doLast = null)
    {
        if (StonesValues.stonesThumbs.Count > 0)
        {
            doLast?.Invoke();
            yield break;
        }

        if (StonesValues.isDownloadingThumbs)
        {
            while (StonesValues.isDownloadingThumbs)
            {
                yield return null;
            }
            doLast?.Invoke();
            yield break;
        }

        StonesValues.isDownloadingThumbs = true;
        yield return StartCoroutine(this.DownloadBundle(new BundleName(this.thumbsBundleName)));
        StonesValues.isDownloadingThumbs = false;

        if (null != doLast)
        {
            doLast();
        }
    }

    public IEnumerator SpawnStoneWithPositionAndRotation(int stoneId, Vector3 sp, Action doLast = null, bool addOffset = false)
    {
        // Check if the stone is already downloaded
        GameObject stone = this.SearchStone(stoneId);

        if (null == stone)
        {
            // If not, download it
            BundleName bundleName = CalculateAssetBundleNameByStoneIndex(stoneId);
            yield return StartCoroutine(this.DownloadBundle(bundleName));
        }
        AssetProps props = StoneSpawnHelper.GetStoneAssetPropsById(stoneId);
        if (addOffset)
            sp += new Vector3(props.offsetX, props.offsetY-1, props.offsetZ);
        Quaternion rt = Quaternion.Euler(props.rotationX, props.rotationY, props.rotationZ);
        GameObject obj = Instantiate(this.SearchStone(stoneId), sp, rt);


        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.materials)
            {
                Shader unlitShader = Shader.Find("Unlit/Texture"); // O "Unlit/Color" si no tiene textura
                if (unlitShader != null)
                {
                    mat.shader = unlitShader;
                }
                else
                {
                    Debug.LogWarning("Unlit shader no encontrado");
                }
            }
        }
        /// 
        /// 
        obj.transform.localScale *= props.scale;

        doLast?.Invoke();
    }

    public GameObject SearchStone(int index)
    {
        string stoneName = this.GetStoneName(index);
        foreach (AssetBundle ab in StonesValues.assetBundles)
        {
            if (this.CheckStoneInBundle(stoneName, ab))
            {
                return ab.LoadAsset<GameObject>(stoneName);
            }
        }

        return null;
    }

    private string GetStoneName(int index)
    {
        return "stone" + index + ".prefab";
    }

    private IEnumerator DownloadBundle(BundleName bundleName)
    {
        #if !UNITY_WEBGL
           UnityEngine.Caching.ClearOtherCachedVersions("anything", new Hash128());
        #endif

        //loadScreen.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Stone download
        if (bundleName.stoneBundleName != null && bundleName.metadataBundleName != null)
        {
            AssetBundle metadataBundle = null;
            AssetBundle stonesBundle = null;

            if (StaticValues.online)
            {
                // Download metadata
                using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(domain + bundleName.metadataBundleName))
                {
                    yield return uwr.SendWebRequest();

                    if (uwr.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(uwr.error);
                        Debug.Log(domain + bundleName.metadataBundleName);
                    }
                    else
                    {
                        // Get downloaded asset bundle
                        Debug.Log("Downloaded metadata bundle" + bundleName.metadataBundleName);
                        metadataBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    }
                }

                // Download Stones
                using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(domain + bundleName.stoneBundleName))
                {
                    yield return uwr.SendWebRequest();

                    if (uwr.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(uwr.error);
                        Debug.Log(domain + bundleName.stoneBundleName);
                    }
                    else
                    {
                        // Get downloaded asset bundle
                        Debug.Log("Downloaded stone bundle " + bundleName.stoneBundleName);
                        stonesBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    }
                }
            }

            // Fallback to local StreamingAssets if metadataBundle failed or offline
            if (metadataBundle == null)
            {
                Debug.Log("Loading offline " + bundleName.metadataBundleName);
#if UNITY_WEBGL
                string path = Application.streamingAssetsPath + "/" + bundleName.metadataBundleName;
                using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(path))
                {
                    yield return uwr.SendWebRequest();
                    if (uwr.result == UnityWebRequest.Result.Success) metadataBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                }
#else
                metadataBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundleName.metadataBundleName));
#endif
            }

            // Fallback to local StreamingAssets if stonesBundle failed or offline
            if (stonesBundle == null)
            {
                Debug.Log("Loading offline " + bundleName.stoneBundleName);
#if UNITY_WEBGL
                string path = Application.streamingAssetsPath + "/" + bundleName.stoneBundleName;
                using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(path))
                {
                    yield return uwr.SendWebRequest();
                    if (uwr.result == UnityWebRequest.Result.Success) stonesBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                }
#else
                stonesBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundleName.stoneBundleName));
#endif
            }

            // Add Bundles to Stones Values
            if (metadataBundle != null)
            {
                StonesValues.metadataAssetBundles.Add(metadataBundle);
            }

            if (stonesBundle != null)
            {
                StonesValues.assetBundles.Add(stonesBundle);
            }
        }
        else if (bundleName.thumbsBundleName != null) // Thumbs download
        {
            // Download thumbs
            AssetBundle thumbsBundle = null;
            if (StaticValues.online)
            {
                using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(domain + bundleName.thumbsBundleName))
                {
                    yield return uwr.SendWebRequest();

                    if (uwr.result != UnityWebRequest.Result.Success)
                    {
                        Debug.Log(uwr.error);
                    }
                    else
                    {
                        // Get downloaded asset bundle
                        Debug.Log("Downloaded thumbs bundle " + bundleName.thumbsBundleName);
                        thumbsBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                    }
                }
            }

            // Fallback to local StreamingAssets if thumbsBundle failed or offline
            if (thumbsBundle == null)
            {
                Debug.Log("Loading offline " + bundleName.thumbsBundleName);
#if UNITY_WEBGL
                string path = Application.streamingAssetsPath + "/" + bundleName.thumbsBundleName;
                using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(path))
                {
                    yield return uwr.SendWebRequest();
                    if (uwr.result == UnityWebRequest.Result.Success) thumbsBundle = DownloadHandlerAssetBundle.GetContent(uwr);
                }
#else
                thumbsBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, bundleName.thumbsBundleName));
#endif
            }

            if (thumbsBundle != null)
            {
                string[] assetNames = thumbsBundle.GetAllAssetNames();
                foreach (string name in assetNames)
                {
                    Texture2D texture = thumbsBundle.LoadAsset<Texture2D>(name);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100.0f);
                    sprite.name = texture.name;
                    StonesValues.stonesThumbs.Add(sprite);
                }
                StonesValues.stonesThumbs.Sort((Sprite p, Sprite q) => Int32.Parse(p.name) - Int32.Parse(q.name));
                thumbsBundle.Unload(false);
            }
        }

        //SloadScreen.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private bool CheckStoneInBundle(string stoneName, AssetBundle ab)
    {
        foreach (string assetName in ab.GetAllAssetNames())
        {
            if (assetName.EndsWith(stoneName))
            {
                return true;
            }
        }
        return false;
    }

    public static BundleName CalculateAssetBundleNameByStoneIndex(int index)
    {
        // Stones start at Stone01
        int init = 1;
        int end = StonesValues.bundleSize;
        while (!(init <= index && index <= end))
        {
            init += StonesValues.bundleSize;
            end += StonesValues.bundleSize;
        }

        string stoneBundleName = "stones_" + init + "_" + end;
        string metadataBundleName = "stones_metadata_" + init + "_" + end;

        return new BundleName(stoneBundleName, metadataBundleName);
    }
}
