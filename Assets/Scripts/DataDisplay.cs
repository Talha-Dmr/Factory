// DataDisplay.cs
using UnityEngine;
using TMPro;

// Bu script, K1, K2, Press gibi her bir makine/kamera objesinin üzerine eklenecek.
// Tek görevi, TimelineManager'dan gelen metni kendi TextMeshPro'sunda göstermektir.
public class DataDisplay : MonoBehaviour
{
    // Inspector'dan her objenin kendi TextMeshPro'sunu sürükleyip býrakýn.
    public TextMeshPro textDisplay;

    private void Start()
    {
        // Baþlangýçta metin kutusunu temizle
        if (textDisplay != null)
        {
            textDisplay.text = "";
        }
    }

    // TimelineManager bu metodu çaðýrarak veriyi güncelleyecek.
    public void UpdateText(string data)
    {
        if (textDisplay != null)
        {
            textDisplay.text = data;
        }
    }
}