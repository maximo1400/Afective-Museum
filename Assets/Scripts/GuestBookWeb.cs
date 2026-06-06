using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class GuestBookWeb : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField messageInput;
    public TextMeshProUGUI outputText;
    public GameObject scrollView;

    private const string postURL = "https://saduewa.dcc.uchile.cl/gamification/guestbook.php";
    private const string getURL = "https://saduewa.dcc.uchile.cl/gamification/get_guestbook.php";

    public void OnSendButton()
    {
        string name = nameInput.text;
        string message = messageInput.text;

        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(message))
        {
            StartCoroutine(SaveEntryToServer(name, message));
        }
    }

    public void OnLoadButton()
    {
        StartCoroutine(LoadEntriesFromServer());
    }

    public void OnCloseButton()
    {
        scrollView.SetActive(false);
    }

    IEnumerator SaveEntryToServer(string name, string message)
    {
        WWWForm form = new WWWForm();
        form.AddField("name", name);
        form.AddField("message", message);

        using (UnityWebRequest www = UnityWebRequest.Post(postURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al guardar: " + www.error);
            }
            else
            {
                Debug.Log("Mensaje guardado.");
                nameInput.text = "";
                messageInput.text = "";
                OnLoadButton(); // recarga los mensajes
            }
        }
    }

    IEnumerator LoadEntriesFromServer()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(getURL))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                outputText.text = "❌ Error al cargar mensajes.";
            }
            else
            {
                string[] lines = www.downloadHandler.text.Split('\n');
                outputText.text = "";
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] parts = lines[i].Split(',');
                    if (parts.Length >= 2)
                    {
                        outputText.text += $"👀 <b>{parts[0]}</b>:\n \t🏡 {parts[1]}\n\n";
                    }
                }

                scrollView.SetActive(true);
            }
        }
    }
}
