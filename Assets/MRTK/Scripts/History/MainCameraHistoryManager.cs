using System;
using System.Collections.Generic;
using UnityEngine;

// List: Sıralı tutup binary search yapacağız
public class MainCameraHistoryManager
{
    private readonly List<TransformHistory> _history = new List<TransformHistory>(4096);
    private const int LinearThreshold = 64;

    public int Count => _history.Count;

    public void Add(TransformHistory data)
    {
        if (_history.Count == 0)
        {
            _history.Add(data);
            return;
        }

        long last = _history[_history.Count - 1].TimeStamp.Ticks;
        long cur = data.TimeStamp.Ticks;

        if (cur >= last)
        {
            _history.Add(data);
            return;
        }

        int idx = LowerBoundByTicks(cur);
        _history.Insert(idx, data);
    }

    public bool Remove(TransformHistory data)
    {
        int idx = IndexOfByTicks(data.TimeStamp.Ticks);
        if (idx < 0) return false;

        for (int i = idx; i >= 0 && _history[i].TimeStamp.Ticks == data.TimeStamp.Ticks; i--)
        {
            if (_history[i].Position == data.Position && _history[i].Rotation == data.Rotation)
            {
                _history.RemoveAt(i);
                return true;
            }
        }
        for (int i = idx + 1; i < _history.Count && _history[i].TimeStamp.Ticks == data.TimeStamp.Ticks; i++)
        {
            if (_history[i].Position == data.Position && _history[i].Rotation == data.Rotation)
            {
                _history.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public bool RemoveByTimeStamp(DateTime ts)
    {
        int idx = IndexOfByTicks(ts.Ticks);
        if (idx < 0) return false;
        _history.RemoveAt(idx);
        return true;
    }

    public void ClearAll() => _history.Clear();

    public TransformHistory? FindClosest(ReceivedData referenceData)
    {
        if (referenceData == null) throw new ArgumentNullException(nameof(referenceData));
        return FindClosest(referenceData.TimeStamp);
    }

    public TransformHistory? FindClosest(DateTime ts)
    {
        if (_history.Count == 0) return null;

        long target = ts.Ticks;

        if (target <= _history[0].TimeStamp.Ticks) return _history[0];
        if (target >= _history[_history.Count - 1].TimeStamp.Ticks) return _history[_history.Count - 1];

        if (_history.Count <= LinearThreshold)
        {
            TransformHistory best = _history[0];
            long bestDiff = Math.Abs(best.TimeStamp.Ticks - target);
            for (int i = 1; i < _history.Count; i++)
            {
                long diff = Math.Abs(_history[i].TimeStamp.Ticks - target);
                if (diff < bestDiff)
                {
                    best = _history[i];
                    bestDiff = diff;
                }
            }
            return best;
        }
        else
        {
            int idx = LowerBoundByTicks(target);
            var a = _history[idx - 1];
            var b = _history[idx];
            long da = Math.Abs(a.TimeStamp.Ticks - target);
            long db = Math.Abs(b.TimeStamp.Ticks - target);
            return (db < da) ? b : a;
        }
    }

    private int LowerBoundByTicks(long target)
    {
        int lo = 0, hi = _history.Count;
        while (lo < hi)
        {
            int mid = (lo + hi) >> 1;
            if (_history[mid].TimeStamp.Ticks < target) lo = mid + 1;
            else hi = mid;
        }
        return lo;
    }

    private int IndexOfByTicks(long target)
    {
        int idx = LowerBoundByTicks(target);
        if (idx < _history.Count && _history[idx].TimeStamp.Ticks == target) return idx;
        return -1;
    }
}

