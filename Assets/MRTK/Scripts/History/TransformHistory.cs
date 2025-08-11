using System;
using UnityEngine;

// Struct: Çok kayıt olduğunda GC baskısını azaltmak için
public readonly struct TransformHistory : IComparable<TransformHistory>
{
    public readonly DateTime TimeStamp;
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;

    public TransformHistory(DateTime ts, Vector3 pos, Quaternion rot)
    {
        TimeStamp = ts;
        Position = pos;
        Rotation = rot;
    }

    public int CompareTo(TransformHistory other) => TimeStamp.Ticks.CompareTo(other.TimeStamp.Ticks);
}
