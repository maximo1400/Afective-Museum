using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;


public class GuestBookManager : MonoBehaviour
{
    public TMP_InputField nameInput;
    public TMP_InputField messageInput;
    public TextMeshProUGUI outputText;
    public GameObject scrollView;
    private string filePath;

    void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "guestbook.csv");

        // Si no existe, crear encabezado
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "Name,Message\n");
        }
    }

    public void SaveEntry()
    {
        string name = nameInput.text.Replace(",", ""); // quitar comas
        string message = messageInput.text.Replace(",", "");
        string line = $"{name},{message}\n";

        File.AppendAllText(filePath, line);

        nameInput.text = "";
        messageInput.text = "";
    }

    public void LoadEntries()
{
    outputText.text = "";

    if (File.Exists(filePath))
    {
        string[] lines = File.ReadAllLines(filePath);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] parts = lines[i].Split(',');
            if (parts.Length >= 2)
            {
                outputText.text += $"👀 <b>{parts[0]}</b>:\n \t🏡 {parts[1]}\n\n";
            }
        }
    }
    else
    {
        outputText.text = "No hay mensajes aún.";
    }

    scrollView.SetActive(true); // Mostrar el scroll al cargar
}

    public void HideScroll()
{
    scrollView.SetActive(false);
}


}
