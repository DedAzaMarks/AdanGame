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
            return Mathf.Lerp(CurrTile.X, NextTile.X, MovementPercentage);
        }
    }

    public float Y
    {
        get
        {
            return Mathf.Lerp(CurrTile.Y, NextTile.Y, MovementPercentage);
        }
    }

    public Tile CurrTile
    {
        get; protected set;
    }


    Tile DestTile;  // If we aren't moving, then destTile = currTile
    Tile NextTile;  // The next tile in the pathfinding sequence
    PathAStar PathAStar;
    float MovementPercentage; // Goes from 0 to 1 as we move from currTile to destTile

    float Speed = 6f;   // Tiles per second

    Action<Character> CbCharacterChanged;

    Job MyJob;

    public Character()
    {
        // Use only for serialization
    }

    public Character(Tile T)
    {
        CurrTile = DestTile = NextTile = T;
    }

    void UpdateDoJob(float deltaTime)
    {
        // Do I have a job?
        if (MyJob == null)
        {
            // Grab a new job.
            MyJob = CurrTile.world.MyJobQueue.Dequeue();

            if (MyJob != null)
            {
                // We have a job!

                // TODO: Check to see if the job is REACHABLE!

                DestTile = MyJob.Tile;
                MyJob.RegisterJobCompleteCallback(OnJobEnded);
                MyJob.RegisterJobCancelCallback(OnJobEnded);
            }
        }

        // Are we there yet?
        if (MyJob != null && CurrTile == MyJob.Tile)
        {
            MyJob.DoWork(deltaTime);
        }

    }

    public void AbandonJob()
    {
        NextTile = DestTile = CurrTile;
        PathAStar = null;
        CurrTile.world.MyJobQueue.Enqueue(MyJob);
        MyJob = null;
    }

    void UpdateDoMovement(float deltaTime)
    {
        if (CurrTile == DestTile)
        {
            PathAStar = null;
            return; // We're already were we want to be.
        }

        // currTile = The tile I am currently in (and may be in the process of leaving)
        // nextTile = The tile I am currently entering
        // destTile = Our final destination -- we never walk here directly, but instead use it for the pathfinding

        if (NextTile == null || NextTile == CurrTile)
        {
            // Get the next tile from the pathfinder.
            if (PathAStar == null || PathAStar.Length() == 0)
            {
                // Generate a path to our destination
                PathAStar = new PathAStar(CurrTile.world, CurrTile, DestTile); // This will calculate a path from curr to dest.
                if (PathAStar.Length() == 0)
                {
                    //Debug.LogError("Path_AStar returned no path to destination!");
                    AbandonJob();
                    PathAStar = null;
                    return;
                }

                // Let's ignore the first tile, because that's the tile we're currently in.
                NextTile = PathAStar.Dequeue();

            }


            // Grab the next waypoint from the pathing system!
            NextTile = PathAStar.Dequeue();

            if (NextTile == CurrTile)
            {
                //Debug.LogError("Update_DoMovement - nextTile is currTile?");
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
        float DistToTravel = Mathf.Sqrt(
            Mathf.Pow(CurrTile.X - NextTile.X, 2) +
            Mathf.Pow(CurrTile.Y - NextTile.Y, 2)
        );

        if (NextTile.IsEnterable() == ENTERABILITY.Never)
        {
            // Most likely a wall got built, so we just need to reset our pathfinding information.
            // FIXME: Ideally, when a wall gets spawned, we should invalidate our path immediately,
            //		  so that we don't waste a bunch of time walking towards a dead end.
            //		  To save CPU, maybe we can only check every so often?
            //		  Or maybe we should register a callback to the OnTileChanged event?
            //Debug.LogError("FIXME: A character was trying to enter an unwalkable tile.");
            NextTile = null;    // our next tile is a no-go
            PathAStar = null;   // clearly our pathfinding info is out of date.
            return;
        }
        else if (NextTile.IsEnterable() == ENTERABILITY.Soon)
        {
            // We can't enter the NOW, but we should be able to in the
            // future. This is likely a DOOR.
            // So we DON'T bail on our movement/path, but we do return
            // now and don't actually process the movement.
            return;
        }

        // How much distance can be travel this Update?
        float distThisFrame = Speed / NextTile.movementCost * deltaTime;

        // How much is that in terms of percentage to our destination?
        float percThisFrame = distThisFrame / DistToTravel;

        // Add that to overall percentage travelled.
        MovementPercentage = MovementPercentage + percThisFrame;

        if (MovementPercentage >= 1)
        {
            // We have reached our destination

            // TODO: Get the next tile from the pathfinding system.
            //       If there are no more tiles, then we have TRULY
            //       reached our destination.

            CurrTile = NextTile;
            MovementPercentage = 0;
            // FIXME?  Do we actually want to retain any overshot movement?
        }


    }

    public void Update(float DeltaTime)
    {
        //Debug.Log("Character Update");

        UpdateDoJob(DeltaTime);

        UpdateDoMovement(DeltaTime);

        if (CbCharacterChanged != null)
            CbCharacterChanged(this);

    }

    public void SetDestination(Tile tile)
    {
        if (CurrTile.IsNeighbour(tile, true) == false)
        {
            Debug.Log("Character::SetDestination -- Our destination tile isn't actually our neighbour.");
        }

        DestTile = tile;
    }

    public void RegisterOnChangedCallback(Action<Character> cb)
    {
        CbCharacterChanged = CbCharacterChanged + cb;
    }

    public void UnregisterOnChangedCallback(Action<Character> cb)
    {
        CbCharacterChanged = CbCharacterChanged - cb;
    }

    void OnJobEnded(Job j)
    {
        // Job completed or was cancelled.

        if (j != MyJob)
        {
            //Debug.LogError("Character being told about job that isn't his. You forgot to unregister something.");
            return;
        }

        MyJob = null;
    }


    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", CurrTile.X.ToString());
        writer.WriteAttributeString("Y", CurrTile.Y.ToString());
    }

    public void ReadXml(XmlReader reader)
    {
    }


}
