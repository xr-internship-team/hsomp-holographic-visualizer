using System;
using NUnit.Framework;
using UnityEngine;

public class MainCameraHistoryManagerTests
{
    [Test]
    public void EmptyList_ReturnsNull()
    {
        var mgr = new MainCameraHistoryManager();
        var rd = new ReceivedData { timestamp = DateTime.UtcNow.ToString("o") };
        Assert.IsNull(mgr.FindClosest(rd));
    }

    [Test]
    public void SingleItem_ReturnsIt()
    {
        var mgr = new MainCameraHistoryManager();
        var ts = DateTime.UtcNow;
        mgr.Add(new TransformHistory(ts, Vector3.zero, Quaternion.identity));
        var rd = new ReceivedData { timestamp = ts.AddMilliseconds(5).ToString("o") };
        Assert.AreEqual(ts.Ticks, mgr.FindClosest(rd)?.TimeStamp.Ticks);
    }

    [Test]
    public void MultipleItems_FindsNearest()
    {
        var mgr = new MainCameraHistoryManager();
        var baseTs = DateTime.UtcNow;
        for (int i = 0; i < 4; i++)
            mgr.Add(new TransformHistory(baseTs.AddMilliseconds(i * 10), Vector3.zero, Quaternion.identity));

        var rd = new ReceivedData { timestamp = baseTs.AddMilliseconds(14).ToString("o") };
        Assert.AreEqual(baseTs.AddMilliseconds(10).Ticks, mgr.FindClosest(rd)?.TimeStamp.Ticks);
    }

    [Test]
    public void LargeSet_PerformanceLog()
    {
        var mgr = new MainCameraHistoryManager();
        var startTs = DateTime.UtcNow;
        for (int i = 0; i < 1_000_000; i++)
            mgr.Add(new TransformHistory(startTs.AddMilliseconds(i), Vector3.zero, Quaternion.identity));

        var rd = new ReceivedData { timestamp = startTs.AddMilliseconds(543_210).ToString("o") };
        var before = DateTime.UtcNow;
        mgr.FindClosest(rd);
        var elapsedMs = (DateTime.UtcNow - before).TotalMilliseconds;
        UnityEngine.Debug.Log($"FindClosest 1M kayıt süresi: {elapsedMs} ms");
    }
}
