using UnityEngine;
using System.Collections.Generic;
using System;

public class JobQueue
{
    Queue<Job> MyJobQueue;

    Action<Job> CbJobCreated;

    public JobQueue()
    {
        MyJobQueue = new Queue<Job>();
    }

    public void Enqueue(Job j)
    {
        MyJobQueue.Enqueue(j);

        if (CbJobCreated != null)
        {
            CbJobCreated(j);
        }
    }

    public Job Dequeue()
    {
        if (MyJobQueue.Count == 0)
            return null;
        
        return MyJobQueue.Dequeue();
    }

    public void RegisterJobCreationCallback(Action<Job> cb)
    {
        CbJobCreated = CbJobCreated + cb;
    }

    public void UnregisterJobCreationCallback(Action<Job> cb)
    {
        CbJobCreated = CbJobCreated - cb;
    }

}
