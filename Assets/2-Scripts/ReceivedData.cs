using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

[Serializable]
public class ReceivedData
{
    public string timestamp;
    public int id;
    public List<float> positionDif;
    public List<float> rotationDif;

    // ... mevcut GetPosition / GetRotation metodların aynı kalsın ...

    // DateTime isteyen kodlar için ek, mevcut kullanımları bozmaz.
    public DateTime TimeStamp
    {
        get
        {
            if (string.IsNullOrEmpty(timestamp)) return DateTime.MinValue;
            // ISO 8601 gibi formatlar için RoundtripKind iyi çalışır: "2025-08-11T01:23:45.678Z"
            if (DateTime.TryParse(timestamp, CultureInfo.InvariantCulture,
                                  DateTimeStyles.RoundtripKind, out var dt))
                return dt;

            // Gerekirse burada farklı format denemeleri eklenebilir.
            return DateTime.MinValue;
        }
    }

    public Vector3 GetPosition()
    {
        return new Vector3(
            positionDif[0],
            positionDif[1],
            positionDif[2]
        );
    }

    public Quaternion GetRotation()
    {
        return new Quaternion(
            rotationDif[0],
            rotationDif[1],
            rotationDif[2],
            rotationDif[3]
        );
    }
}
