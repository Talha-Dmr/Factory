using UnityEngine;
using TMPro;           // or UnityEngine.UI for standard Text
using System.Collections.Generic;

public class TriggerShowData : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public TextMeshProUGUI dataText;    
    public TextAsset csvFile;

    // internal storage
    private List<string> lines;

    void Awake()
    {
        // Parse CSV into lines
        if (csvFile != null)
            lines = new List<string>(csvFile.text.Split('\n'));
        else
            Debug.LogError("No CSV assigned to TriggerShowData!");

        // Hide text initially
        if (dataText != null)
            dataText.text = "";
    }

    void OnTriggerEnter(Collider other)
    {
        // only react to your player
        if (other.CompareTag("Player"))
        {
            ShowAllData();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            dataText.text = "";  // clear when leaving
        }
    }

    void ShowAllData()
    {
        if (lines == null || dataText == null) return;

        // Build a display string
        var display = new System.Text.StringBuilder();
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split(',');
            display.AppendLine(string.Join(" | ", cols));
        }

        dataText.text = display.ToString();
    }
}
