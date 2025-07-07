using UnityEngine;
using TMPro;
using System.Collections;

public class PressScript : MonoBehaviour
{
    public TextMeshPro textDisplay;
    public TextAsset csvFile;
    public float delaySeconds = 0.5f;

    void Start()
    {
        if (csvFile == null || textDisplay == null)
        {
            Debug.LogError("CSV file or Text Display is missing.");
            return;
        }

        StartCoroutine(DisplayCSVDataDelayed());
    }

    IEnumerator DisplayCSVDataDelayed()
    {
        string[] lines = csvFile.text.Split('\n');
        textDisplay.text = ""; // Clear the display

        foreach (string line in lines)
        {
            string cleanedLine = line.Trim();
            if (!string.IsNullOrEmpty(cleanedLine))
            {
                string[] values = cleanedLine.Split(',');
                foreach (string cell in values)
                {
                    textDisplay.text += cell.Trim() + " | ";
                    yield return new WaitForSeconds(delaySeconds);
                }

                textDisplay.text += "\n";
                yield return new WaitForSeconds(delaySeconds);
            }
        }
    }
}
