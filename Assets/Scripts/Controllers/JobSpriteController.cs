using UnityEngine;
using System.Collections.Generic;

public class JobSpriteController : MonoBehaviour
{

    // This bare-bones controller is mostly just going to piggyback
    // on FurnitureSpriteController because we don't yet fully know
    // what our job system is going to look like in the end.

    FurnitureSpriteController fsc;
    Dictionary<Job, GameObject> jobGameObjectMap;

    // Use this for initialization
    void Start()
    {
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();

        // FIXME: No such thing as a job queue yet!
        WorldController.Instance.World.MyJobQueue.RegisterJobCreationCallback(OnJobCreated);
    }

    void OnJobCreated(Job Job)
    {
        // FIXME: We can only do furniture-building jobs.

        // TODO: Sprite


        if (jobGameObjectMap.ContainsKey(Job))
        {
            //Debug.LogError("OnJobCreated for a jobGO that already exists -- most likely a job being RE-QUEUED, as opposed to created.");
            return;
        }

        GameObject JobGo = new GameObject();

        // Add our tile/GO pair to the dictionary.
        jobGameObjectMap.Add(Job, JobGo);

        JobGo.name = "JOB_" + Job.JobObjectType + "_" + Job.Tile.X + "_" + Job.Tile.Y;
        JobGo.transform.position = new Vector3(Job.Tile.X, Job.Tile.Y, 0);
        JobGo.transform.SetParent(this.transform, true);

        SpriteRenderer SR = JobGo.AddComponent<SpriteRenderer>();
        SR.sprite = fsc.GetSpriteForFurniture(Job.JobObjectType);
        SR.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        SR.sortingLayerName = "Jobs";

        Job.RegisterJobCompleteCallback(OnJobEnded);
        Job.RegisterJobCancelCallback(OnJobEnded);
    }

    void OnJobEnded(Job job)
    {
        // This executes whether a job was COMPLETED or CANCELLED

        // FIXME: We can only do furniture-building jobs.

        GameObject jobGo = jobGameObjectMap[job];

        job.UnregisterJobCompleteCallback(OnJobEnded);
        job.UnregisterJobCancelCallback(OnJobEnded);

        Destroy(jobGo);

    }
}
