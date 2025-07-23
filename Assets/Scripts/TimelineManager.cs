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

    private Dictionary<string, DataDisplay> dataDisplays = new Dictionary<string, DataDisplay>();
    private List<TimestampedEvent> timelineEvents = new List<TimestampedEvent>();

    private float gameTimer = 0f;
    private int nextEventIndex = 0;
    private bool isTimelineReady = false;

    private struct TimestampedEvent
    {
        public float EventTime;
        public string TargetID;
        public string Value;
    }

    void Start()
    {
        DataDisplay[] allDisplays = FindObjectsByType<DataDisplay>(FindObjectsSortMode.None);
        foreach (var display in allDisplays)
        {
            if (!dataDisplays.ContainsKey(display.gameObject.name))
            {
                dataDisplays.Add(display.gameObject.name, display);
            }
        }

        if (cameraCsvFile != null && machineCsvFile != null)
        {
            DateTime startTime = FindOverallStartTime();
            ParseCameraData(startTime);
            ParseMachineData(startTime);

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

        gameTimer += Time.deltaTime;

        while (nextEventIndex < timelineEvents.Count && gameTimer >= timelineEvents[nextEventIndex].EventTime)
        {
            TimestampedEvent currentEvent = timelineEvents[nextEventIndex];

            if (dataDisplays.TryGetValue(currentEvent.TargetID, out DataDisplay display))
            {
                display.UpdateText(currentEvent.Value);
                Debug.Log($"Zaman: {gameTimer:F2}s | Tetiklenen: {currentEvent.TargetID} | Deðer: {currentEvent.Value}");
            }

            nextEventIndex++;
        }
    }

    private DateTime FindOverallStartTime()
    {
        DateTime cameraMin = ParseFirstTimestamp(cameraCsvFile, 5); // camera.csv'de timestamp 6. sütunda (index 5) 
        DateTime machineMin = ParseFirstTimestamp(machineCsvFile, 6); // machine.csv'de timestamp 7. sütunda (index 6)

        return cameraMin < machineMin ? cameraMin : machineMin;
    }

    private DateTime ParseFirstTimestamp(TextAsset file, int timestampColumnIndex)
    {
        string[] lines = file.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 1)
        {
            string[] fields = lines[1].Split(',');
            if (fields.Length > timestampColumnIndex)
            {
                try
                {
                    return ParseTimestamp(fields[timestampColumnIndex]);
                }
                catch (FormatException e)
                {
                    Debug.LogWarning($"Ýlk timestamp parse edilemedi: {fields[timestampColumnIndex]}. Hata: {e.Message}");
                }
            }
        }
        return DateTime.MaxValue;
    }

    void ParseCameraData(DateTime startTime)
    {
        string[] lines = cameraCsvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            if (fields.Length < 6) continue;

            string barcode = fields[4];
            string cameraID = fields[3];
            string timestampStr = fields[5];

            try
            {
                DateTime timestamp = ParseTimestamp(timestampStr);
                timelineEvents.Add(new TimestampedEvent
                {
                    EventTime = (float)(timestamp - startTime).TotalSeconds,
                    TargetID = cameraID,
                    Value = barcode
                });
            }
            catch (FormatException e)
            {
                Debug.LogWarning($"Satýr {i + 1} (CameraData) parse edilemedi. Deðer: '{timestampStr}'. Hata: {e.Message}");
            }
        }
    }

    void ParseMachineData(DateTime startTime)
    {
        string[] lines = machineCsvFile.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split(',');
            if (fields.Length < 7) continue;

            string machineID = fields[3];
            string parameter = fields[4];
            string value = fields[5];
            string timestampStr = fields[6];

            try
            {
                DateTime timestamp = ParseTimestamp(timestampStr);
                timelineEvents.Add(new TimestampedEvent
                {
                    EventTime = (float)(timestamp - startTime).TotalSeconds,
                    TargetID = machineID,
                    Value = $"{parameter}: {value}"
                });
            }
            catch (FormatException e)
            {
                Debug.LogWarning($"Satýr {i + 1} (MachineData) parse edilemedi. Deðer: '{timestampStr}'. Hata: {e.Message}");
            }
        }
    }

    // GÜNCELLENMÝÞ FONKSÝYON
    private DateTime ParseTimestamp(string timestampStr)
    {
        // Farklý formatlarý denemek için bir format dizisi oluþturuyoruz.
        string[] formats = {
            "yyyy-MM-dd HH:mm:ss.ffffff+00:00",
            "yyyy-MM-dd HH:mm:ss+00:00"
        };

        return DateTime.ParseExact(timestampStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }
}