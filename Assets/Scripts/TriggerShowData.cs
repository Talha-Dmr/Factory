using UnityEngine;
using TMPro;

public class TriggerShowData : MonoBehaviour
{
    [Header("Text ve CSV Ayarları")]
    public TextMeshPro textDisplay;
    public TextAsset csvFile;
    public float lockDuration = 2f;

    private string[] lines;
    private int currentLine = 0;
    private bool isLocked = false;
    private float lockTimer = 0f;

    private void Start()
    {
        if (textDisplay != null)
            textDisplay.text = ""; // Başlangıçta boş

        if (csvFile != null)
            lines = csvFile.text.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.RemoveEmptyEntries);
        else
            Debug.LogError("CSV dosyası atanmadı!");
    }

    private void Update()
    {
        if (isLocked)
        {
            lockTimer += Time.deltaTime;
            if (lockTimer >= lockDuration)
            {
                isLocked = false;
                lockTimer = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isLocked)
        {
            ShowNextLine();
            isLocked = true;
            lockTimer = 0f;
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
