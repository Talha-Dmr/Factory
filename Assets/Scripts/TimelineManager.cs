// TimelineManager.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class TimelineManager : MonoBehaviour
{
    [Header("CSV Dosyalar�")]
    public TextAsset cameraCsvFile;
    public TextAsset machineCsvFile;

    // Sahnedeki t�m DataDisplay script'lerini tutan bir dictionary (s�zl�k).
    // Anahtar: Makine/Kamera ID'si (�rn: "K1", "Press")
    // De�er: O objeye ait DataDisplay script'i
    private Dictionary<string, DataDisplay> dataDisplays = new Dictionary<string, DataDisplay>();

    // T�m olaylar� (kamera ve makine) zaman s�ras�na g�re tutan liste.
    private List<TimestampedEvent> timelineEvents = new List<TimestampedEvent>();

    private float gameTimer = 0f;
    private int nextEventIndex = 0;
    private bool isTimelineReady = false;

    // T�m olaylar� temsil eden basit bir struct yap�s�.
    private struct TimestampedEvent
    {
        public float EventTime; // Olay�n ger�ekle�ece�i oyun zaman� (saniye cinsinden)
        public string TargetID;   // Hangi nesneyi etkileyecek (�rn: "K1", "Press")
        public string Value;      // G�sterilecek de�er (�rn: barkod veya makine de�eri)
    }

    void Start()
    {
        // 1. Sahnedeki t�m DataDisplay script'lerini bul ve dictionary'e ekle.
        // DE����KL�K BURADA: FindObjectsOfType yerine FindObjectsByType kullan�ld�.
        DataDisplay[] allDisplays = FindObjectsByType<DataDisplay>(FindObjectsSortMode.None);
        foreach (var display in allDisplays)
        {
            // E�er ayn� isimde obje yoksa ekle
            if (!dataDisplays.ContainsKey(display.gameObject.name))
            {
                dataDisplays.Add(display.gameObject.name, display);
            }
        }

        // 2. CSV dosyalar�n� oku ve olaylar� olu�tur.
        if (cameraCsvFile != null && machineCsvFile != null)
        {
            DateTime startTime = FindOverallStartTime();
            ParseCameraData(startTime);
            ParseMachineData(startTime);

            // 3. T�m olaylar� zamanlar�na g�re k���kten b�y��e s�rala.
            timelineEvents = timelineEvents.OrderBy(e => e.EventTime).ToList();

            isTimelineReady = true;
            Debug.Log($"Timeline haz�rland�. Toplam {timelineEvents.Count} olay bulundu.");
        }
        else
        {
            Debug.LogError("CSV dosyalar� atanmam��!");
        }
    }

    void Update()
    {
        if (!isTimelineReady) return;

        // Oyun zaman�n� art�r.
        gameTimer += Time.deltaTime;

        // Zaman� gelen olaylar� tetikle.
        // 'while' kullan�yoruz ��nk� bir frame'de birden fazla olay�n zaman� gelebilir (�zellikle d���k FPS'de).
        while (nextEventIndex < timelineEvents.Count && gameTimer >= timelineEvents[nextEventIndex].EventTime)
        {
            TimestampedEvent currentEvent = timelineEvents[nextEventIndex];

            // �lgili DataDisplay script'ini bul ve metnini g�ncelle.
            if (dataDisplays.TryGetValue(currentEvent.TargetID, out DataDisplay display))
            {
                display.UpdateText(currentEvent.Value);
                Debug.Log($"Zaman: {gameTimer:F2}s | Tetiklenen: {currentEvent.TargetID} | De�er: {currentEvent.Value}");
            }

            nextEventIndex++;
        }
    }

    // Her iki CSV'yi de taray�p en erken timestamp'i bulan yard�mc� fonksiyon
    private DateTime FindOverallStartTime()
    {
        DateTime cameraMin = ParseFirstTimestamp(cameraCsvFile, 2); // timestamp s�tunu 3. s�rada (index 2)
        DateTime machineMin = ParseFirstTimestamp(machineCsvFile, 6); // timestamp s�tunu 7. s�rada (index 6)

        return cameraMin < machineMin ? cameraMin : machineMin;
    }

    private DateTime ParseFirstTimestamp(TextAsset file, int timestampColumnIndex)
    {
        string[] lines = file.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 1) // Header'� atla
        {
            string[] fields = lines[1].Split(',');
            if (fields.Length > timestampColumnIndex)
            {
                return ParseTimestamp(fields[timestampColumnIndex]);
            }
        }
        return DateTime.MaxValue; // Hata durumunda �ok b�y�k bir de�er d�nd�r
    }

    void ParseCameraData(DateTime startTime)
    {
        string[] lines = cameraCsvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        // Header sat�r�n� atlamak i�in i=1'den ba�lat
        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            if (fields.Length < 3) continue; // Eksik veri olan sat�rlar� atla

            string barcode = fields[0];
            string cameraID = fields[1];
            DateTime timestamp = ParseTimestamp(fields[2]);

            timelineEvents.Add(new TimestampedEvent
            {
                EventTime = (float)(timestamp - startTime).TotalSeconds,
                TargetID = cameraID,
                Value = barcode
            });
        }
    }

    void ParseMachineData(DateTime startTime)
    {
        string[] lines = machineCsvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        // Header sat�r�n� atlamak i�in i=1'den ba�lat
        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            // Unnamed, factoryId, lineId, machineId, parameter, value, timestamp
            if (fields.Length < 7) continue;

            string machineID = fields[3];
            string parameter = fields[4];
            string value = fields[5];
            DateTime timestamp = ParseTimestamp(fields[6]);

            timelineEvents.Add(new TimestampedEvent
            {
                EventTime = (float)(timestamp - startTime).TotalSeconds,
                TargetID = machineID,
                // De�eri "parametre: de�er" format�nda g�sterebiliriz
                Value = $"{parameter}: {value}"
            });
        }
    }

    // CSV'deki timestamp format�n� DateTime objesine �eviren fonksiyon.
    // Format: "2025-07-02 00:00:07.590000+00:00"
    private DateTime ParseTimestamp(string timestampStr)
    {
        // Unity'nin anlayaca�� standart bir formata getirelim.
        // ".ffffff" ve "+00:00" k�s�mlar� DateTime.Parse i�in sorun yaratabilir,
        // bu y�zden "o" (round-trip) format specifier'�n� kullanmak en g�venlisidir.
        return DateTime.Parse(timestampStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }
}