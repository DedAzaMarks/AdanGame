using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Character : IXmlSerializable
{
    public float X
    {
        get
        {
            return Mathf.Lerp(currTile.X, nextTile.X, movementPercentage);
        }
    }

    public float Y
    {
        get
        {
            return Mathf.Lerp(currTile.Y, nextTile.Y, movementPercentage);
        }
    }

    public Tile currTile
    {
        get; protected set;
    }


    Tile destTile;  // If we aren't moving, then destTile = currTile
    Tile nextTile;  // The next tile in the pathfinding sequence
    Path_AStar pathAStar;
    float movementPercentage; // Goes from 0 to 1 as we move from currTile to destTile

    float speed = 6f;   // Tiles per second

    Action<Character> cbCharacterChanged;

    Job myJob;

    public Character()
    {
        // Use only for serialization
    }

    public Character(Tile tile)
    {
        currTile = destTile = nextTile = tile;
    }

    void Update_DoJob(float deltaTime)
    {
        // Do I have a job?
        if (myJob == null)
        {
            // Grab a new job.
            myJob = currTile.world.jobQueue.Dequeue();

            if (myJob != null)
            {
                // We have a job!

                // TODO: Check to see if the job is REACHABLE!

                destTile = myJob.tile;
                myJob.RegisterJobCompleteCallback(OnJobEnded);
                myJob.RegisterJobCancelCallback(OnJobEnded);
            }
        }

        // Are we there yet?
        if (myJob != null && currTile == myJob.tile)
        {
            myJob.DoWork(deltaTime);
        }

    }

    public void AbandonJob()
    {
        nextTile = destTile = currTile;
        pathAStar = null;
        currTile.world.jobQueue.Enqueue(myJob);
        myJob = null;
    }

    void Update_DoMovement(float deltaTime)
    {
        if (currTile == destTile)
        {
            pathAStar = null;
            return; // We're already were we want to be.
        }

        // currTile = The tile I am currently in (and may be in the process of leaving)
        // nextTile = The tile I am currently entering
        // destTile = Our final destination -- we never walk here directly, but instead use it for the pathfinding

        if (nextTile == null || nextTile == currTile)
        {
            // Get the next tile from the pathfinder.
            if (pathAStar == null || pathAStar.Length() == 0)
            {
                // Generate a path to our destination
                pathAStar = new Path_AStar(currTile.world, currTile, destTile); // This will calculate a path from curr to dest.
                if (pathAStar.Length() == 0)
                {
                    Debug.LogError("Path_AStar returned no path to destination!");
                    AbandonJob();
                    pathAStar = null;
                    return;
                }

                // Let's ignore the first tile, because that's the tile we're currently in.
                nextTile = pathAStar.Dequeue();

            }


            // Grab the next waypoint from the pathing system!
            nextTile = pathAStar.Dequeue();

            if (nextTile == currTile)
            {
                Debug.LogError("Update_DoMovement - nextTile is currTile?");
            }
        }

        /*		if(pathAStar.Length() == 1) {
                    return;
                }
        */
        // At this point we should have a valid nextTile to move to.

        // What's the total distance from point A to point B?
        // We are going to use Euclidean distance FOR NOW...
        // But when we do the pathfinding system, we'll likely
        // switch to something like Manhattan or Chebyshev distance
        float distToTravel = Mathf.Sqrt(
            Mathf.Pow(currTile.X - nextTile.X, 2) +
            Mathf.Pow(currTile.Y - nextTile.Y, 2)
        );

        if (nextTile.IsEnterable() == ENTERABILITY.Never)
        {
            // Most likely a wall got built, so we just need to reset our pathfinding information.
            // FIXME: Ideally, when a wall gets spawned, we should invalidate our path immediately,
            //		  so that we don't waste a bunch of time walking towards a dead end.
            //		  To save CPU, maybe we can only check every so often?
            //		  Or maybe we should register a callback to the OnTileChanged event?
            Debug.LogError("FIXME: A character was trying to enter an unwalkable tile.");
            nextTile = null;    // our next tile is a no-go
            pathAStar = null;   // clearly our pathfinding info is out of date.
            return;
        }
        else if (nextTile.IsEnterable() == ENTERABILITY.Soon)
        {
            // We can't enter the NOW, but we should be able to in the
            // future. This is likely a DOOR.
            // So we DON'T bail on our movement/path, but we do return
            // now and don't actually process the movement.
            return;
        }

        // How much distance can be travel this Update?
        float distThisFrame = speed / nextTile.movementCost * deltaTime;

        // How much is that in terms of percentage to our destination?
        float percThisFrame = distThisFrame / distToTravel;

        // Add that to overall percentage travelled.
        movementPercentage += percThisFrame;

        if (movementPercentage >= 1)
        {
            // We have reached our destination

            // TODO: Get the next tile from the pathfinding system.
            //       If there are no more tiles, then we have TRULY
            //       reached our destination.

            currTile = nextTile;
            movementPercentage = 0;
            // FIXME?  Do we actually want to retain any overshot movement?
        }


    }

    public void Update(float deltaTime)
    {
        //Debug.Log("Character Update");

        Update_DoJob(deltaTime);

        Update_DoMovement(deltaTime);

        if (cbCharacterChanged != null)
            cbCharacterChanged(this);

    }

    public void SetDestination(Tile tile)
    {
        if (currTile.IsNeighbour(tile, true) == false)
        {
            Debug.Log("Character::SetDestination -- Our destination tile isn't actually our neighbour.");
        }

        destTile = tile;
    }

    public void RegisterOnChangedCallback(Action<Character> cb)
    {
        cbCharacterChanged += cb;
    }

    public void UnregisterOnChangedCallback(Action<Character> cb)
    {
        cbCharacterChanged -= cb;
    }

    void OnJobEnded(Job j)
    {
        // Job completed or was cancelled.

        if (j != myJob)
        {
            Debug.LogError("Character being told about job that isn't his. You forgot to unregister something.");
            return;
        }

        myJob = null;
    }


    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", currTile.X.ToString());
        writer.WriteAttributeString("Y", currTile.Y.ToString());
    }

    public void ReadXml(XmlReader reader)
    {
    }


}
