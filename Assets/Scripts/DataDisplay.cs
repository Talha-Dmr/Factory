// DataDisplay.cs
using UnityEngine;
using TMPro;

// Bu script, K1, K2, Press gibi her bir makine/kamera objesinin �zerine eklenecek.
// Tek g�revi, TimelineManager'dan gelen metni kendi TextMeshPro'sunda g�stermektir.
public class DataDisplay : MonoBehaviour
{
    // Inspector'dan her objenin kendi TextMeshPro'sunu s�r�kleyip b�rak�n.
    public TextMeshPro textDisplay;

    private void Start()
    {
        // Ba�lang��ta metin kutusunu temizle
        if (textDisplay != null)
        {
            textDisplay.text = "";
        }
    }

    // TimelineManager bu metodu �a��rarak veriyi g�ncelleyecek.
    public void UpdateText(string data)
    {
        if (textDisplay != null)
        {
            textDisplay.text = data;
        }
    }
}