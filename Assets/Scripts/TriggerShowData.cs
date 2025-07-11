using UnityEngine;
using TMPro;

public class TriggerShowData : MonoBehaviour
{
    [Header("Text ve CSV Ayarları")]
    public TextMeshPro textDisplay;
    public TextAsset csvFile;

    private string[] lines;
    private int currentLine = 0;

    private void Start()
    {
        if (textDisplay != null)
            textDisplay.text = ""; // Başlangıçta boş

        if (csvFile != null)
            lines = csvFile.text.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries);
        else
            Debug.LogError("CSV dosyası atanmadı!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ShowNextLine();
        }
    }

    private void ShowNextLine()
    {
        if (lines != null && currentLine < lines.Length)
        {
            textDisplay.text = lines[currentLine];
            currentLine++; // Sonraki trigger için artır
        }
        else
        {
            textDisplay.text = "Tüm satırlar bitti!";
        }
    }
}
