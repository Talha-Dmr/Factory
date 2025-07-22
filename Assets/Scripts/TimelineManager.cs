// TimelineManager.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class TimelineManager : MonoBehaviour
{
    [Header("CSV Dosyalarý")]
    public TextAsset cameraCsvFile;
    public TextAsset machineCsvFile;

    // Sahnedeki tüm DataDisplay script'lerini tutan bir dictionary (sözlük).
    // Anahtar: Makine/Kamera ID'si (örn: "K1", "Press")
    // Deðer: O objeye ait DataDisplay script'i
    private Dictionary<string, DataDisplay> dataDisplays = new Dictionary<string, DataDisplay>();

    // Tüm olaylarý (kamera ve makine) zaman sýrasýna göre tutan liste.
    private List<TimestampedEvent> timelineEvents = new List<TimestampedEvent>();

    private float gameTimer = 0f;
    private int nextEventIndex = 0;
    private bool isTimelineReady = false;

    // Tüm olaylarý temsil eden basit bir struct yapýsý.
    private struct TimestampedEvent
    {
        public float EventTime; // Olayýn gerçekleþeceði oyun zamaný (saniye cinsinden)
        public string TargetID;   // Hangi nesneyi etkileyecek (örn: "K1", "Press")
        public string Value;      // Gösterilecek deðer (örn: barkod veya makine deðeri)
    }

    void Start()
    {
        // 1. Sahnedeki tüm DataDisplay script'lerini bul ve dictionary'e ekle.
        // DEÐÝÞÝKLÝK BURADA: FindObjectsOfType yerine FindObjectsByType kullanýldý.
        DataDisplay[] allDisplays = FindObjectsByType<DataDisplay>(FindObjectsSortMode.None);
        foreach (var display in allDisplays)
        {
            // Eðer ayný isimde obje yoksa ekle
            if (!dataDisplays.ContainsKey(display.gameObject.name))
            {
                dataDisplays.Add(display.gameObject.name, display);
            }
        }

        // 2. CSV dosyalarýný oku ve olaylarý oluþtur.
        if (cameraCsvFile != null && machineCsvFile != null)
        {
            DateTime startTime = FindOverallStartTime();
            ParseCameraData(startTime);
            ParseMachineData(startTime);

            // 3. Tüm olaylarý zamanlarýna göre küçükten büyüðe sýrala.
            timelineEvents = timelineEvents.OrderBy(e => e.EventTime).ToList();

            isTimelineReady = true;
            Debug.Log($"Timeline hazýrlandý. Toplam {timelineEvents.Count} olay bulundu.");
        }
        else
        {
            Debug.LogError("CSV dosyalarý atanmamýþ!");
        }
    }

    void Update()
    {
        if (!isTimelineReady) return;

        // Oyun zamanýný artýr.
        gameTimer += Time.deltaTime;

        // Zamaný gelen olaylarý tetikle.
        // 'while' kullanýyoruz çünkü bir frame'de birden fazla olayýn zamaný gelebilir (özellikle düþük FPS'de).
        while (nextEventIndex < timelineEvents.Count && gameTimer >= timelineEvents[nextEventIndex].EventTime)
        {
            TimestampedEvent currentEvent = timelineEvents[nextEventIndex];

            // Ýlgili DataDisplay script'ini bul ve metnini güncelle.
            if (dataDisplays.TryGetValue(currentEvent.TargetID, out DataDisplay display))
            {
                display.UpdateText(currentEvent.Value);
                Debug.Log($"Zaman: {gameTimer:F2}s | Tetiklenen: {currentEvent.TargetID} | Deðer: {currentEvent.Value}");
            }

            nextEventIndex++;
        }
    }

    // Her iki CSV'yi de tarayýp en erken timestamp'i bulan yardýmcý fonksiyon
    private DateTime FindOverallStartTime()
    {
        DateTime cameraMin = ParseFirstTimestamp(cameraCsvFile, 2); // timestamp sütunu 3. sýrada (index 2)
        DateTime machineMin = ParseFirstTimestamp(machineCsvFile, 6); // timestamp sütunu 7. sýrada (index 6)

        return cameraMin < machineMin ? cameraMin : machineMin;
    }

    private DateTime ParseFirstTimestamp(TextAsset file, int timestampColumnIndex)
    {
        string[] lines = file.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 1) // Header'ý atla
        {
            string[] fields = lines[1].Split(',');
            if (fields.Length > timestampColumnIndex)
            {
                return ParseTimestamp(fields[timestampColumnIndex]);
            }
        }
        return DateTime.MaxValue; // Hata durumunda çok büyük bir deðer döndür
    }

    void ParseCameraData(DateTime startTime)
    {
        string[] lines = cameraCsvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        // Header satýrýný atlamak için i=1'den baþlat
        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            if (fields.Length < 3) continue; // Eksik veri olan satýrlarý atla

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
        // Header satýrýný atlamak için i=1'den baþlat
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
                // Deðeri "parametre: deðer" formatýnda gösterebiliriz
                Value = $"{parameter}: {value}"
            });
        }
    }

    // CSV'deki timestamp formatýný DateTime objesine çeviren fonksiyon.
    // Format: "2025-07-02 00:00:07.590000+00:00"
    private DateTime ParseTimestamp(string timestampStr)
    {
        // Unity'nin anlayacaðý standart bir formata getirelim.
        // ".ffffff" ve "+00:00" kýsýmlarý DateTime.Parse için sorun yaratabilir,
        // bu yüzden "o" (round-trip) format specifier'ýný kullanmak en güvenlisidir.
        return DateTime.Parse(timestampStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }
}