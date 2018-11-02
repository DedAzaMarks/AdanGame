using UnityEngine;
using System.Collections.Generic;
using System;

public class Job
{
    // This class holds info for a queued up job, which can include
    // things like placing furniture, moving stored inventory,
    // working at a desk, and maybe even fighting enemies.

    public Tile Tile { get; protected set; }
    float JobTime;

    public string JobObjectType
    {
        get; protected set;
    }

    Action<Job> CbJobComplete;
    Action<Job> CbJobCancel;

    public Job(Tile Tile, string JobObjectType, Action<Job> CbJobComplete, float JobTime = 0.1f)
    {
        this.Tile = Tile;
        this.JobObjectType = JobObjectType;
        this.CbJobComplete = this.CbJobComplete + CbJobComplete;
        this.JobTime = JobTime;
    }

    public void RegisterJobCompleteCallback(Action<Job> cb)
    {
        CbJobComplete = CbJobComplete + cb;
    }

    public void RegisterJobCancelCallback(Action<Job> cb)
    {
        CbJobCancel = CbJobCancel + cb;
    }

    public void UnregisterJobCompleteCallback(Action<Job> cb)
    {
        CbJobComplete = CbJobComplete - cb;
    }

    public void UnregisterJobCancelCallback(Action<Job> cb)
    {
        CbJobCancel = CbJobCancel - cb;
    }

    public void DoWork(float WorkTime)
    {
        JobTime = JobTime - WorkTime;

        if (JobTime <= 0)
        {
            if (CbJobComplete != null)
                CbJobComplete(this);
        }
    }

    public void CancelJob()
    {
        if (CbJobCancel != null)
            CbJobCancel(this);
    }
}
