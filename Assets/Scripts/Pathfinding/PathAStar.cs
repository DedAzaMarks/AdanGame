using UnityEngine;
using System.Collections.Generic;
using PriorityQueue;
using System.Linq;

public class PathAStar
{

    Queue<Tile> path;

    public PathAStar(World world, Tile tileStart, Tile tileEnd)
    {

        // Check to see if we have a valid tile graph
        if (world.TileGraph == null)
        {
            world.TileGraph = new PathTileGraph(world);
        }

        // A dictionary of all valid, walkable nodes.
        Dictionary<Tile, PathNode<Tile>> nodes = world.TileGraph.nodes;

        // Make sure our start/end tiles are in the list of nodes!
        if (nodes.ContainsKey(tileStart) == false)
        {
            //Debug.LogError("Path_AStar: The starting tile isn't in the list of nodes!");

            return;
        }
        if (nodes.ContainsKey(tileEnd) == false)
        {
            //Debug.LogError("Path_AStar: The ending tile isn't in the list of nodes!");
            return;
        }


        PathNode<Tile> start = nodes[tileStart];
        PathNode<Tile> goal = nodes[tileEnd];


        // Mostly following this pseusocode:
        // https://en.wikipedia.org/wiki/A*_search_algorithm

        List<PathNode<Tile>> ClosedSet = new List<PathNode<Tile>>();

        /*		List<Path_Node<Tile>> OpenSet = new List<Path_Node<Tile>>();
                OpenSet.Add( start );
        */

        SimplePriorityQueue<PathNode<Tile>> OpenSet = new SimplePriorityQueue<PathNode<Tile>>();
        OpenSet.Enqueue(start, 0);

        Dictionary<PathNode<Tile>, PathNode<Tile>> CameFrom = new Dictionary<PathNode<Tile>, PathNode<Tile>>();

        Dictionary<PathNode<Tile>, float> gScore = new Dictionary<PathNode<Tile>, float>();
        foreach (PathNode<Tile> n in nodes.Values)
        {
            gScore[n] = Mathf.Infinity;
        }
        gScore[start] = 0;

        Dictionary<PathNode<Tile>, float> fScore = new Dictionary<PathNode<Tile>, float>();
        foreach (PathNode<Tile> n in nodes.Values)
        {
            fScore[n] = Mathf.Infinity;
        }
        fScore[start] = heuristicCostEstimate(start, goal);

        while (OpenSet.Count > 0)
        {
            PathNode<Tile> current = OpenSet.Dequeue();

            if (current == goal)
            {
                // We have reached our goal!
                // Let's convert this into an actual sequene of
                // tiles to walk on, then end this constructor function!
                ReconstructPath(CameFrom, current);
                return;
            }

            ClosedSet.Add(current);

            foreach (PathEdge<Tile> edgeNeighbor in current.edges)
            {
                PathNode<Tile> neighbor = edgeNeighbor.node;

                if (ClosedSet.Contains(neighbor) == true)
                    continue; // ignore this already completed neighbor

                float movementCostToNeighbor = neighbor.data.movementCost * distBetween(current, neighbor);

                float tentativeGScore = gScore[current] + movementCostToNeighbor;

                if (OpenSet.Contains(neighbor) && tentativeGScore >= gScore[neighbor])
                    continue;

                CameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + heuristicCostEstimate(neighbor, goal);

                if (OpenSet.Contains(neighbor) == false)
                {
                    OpenSet.Enqueue(neighbor, fScore[neighbor]);
                }
                else
                {
                    OpenSet.UpdatePriority(neighbor, fScore[neighbor]);
                }

            } // foreach neighbour
        } // while

        // If we reached here, it means that we've burned through the entire
        // OpenSet without ever reaching a point where current == goal.
        // This happens when there is no path from start to goal
        // (so there's a wall or missing floor or something).

        // We don't have a failure state, maybe? It's just that the
        // path list will be null.
    }

    float heuristicCostEstimate(PathNode<Tile> a, PathNode<Tile> b)
    {

        return Mathf.Sqrt(
            Mathf.Pow(a.data.X - b.data.X, 2) +
            Mathf.Pow(a.data.Y - b.data.Y, 2)
        );

    }

    float distBetween(PathNode<Tile> a, PathNode<Tile> b)
    {
        // We can make assumptions because we know we're working
        // on a grid at this point.

        // Hori/Vert neighbours have a distance of 1
        if (Mathf.Abs(a.data.X - b.data.X) + Mathf.Abs(a.data.Y - b.data.Y) == 1)
        {
            return 1f;
        }

        // Diag neighbours have a distance of 1.41421356237	
        if (Mathf.Abs(a.data.X - b.data.X) == 1 && Mathf.Abs(a.data.Y - b.data.Y) == 1)
        {
            return 1.41421356237f;
        }

        // Otherwise, do the actual math.
        return Mathf.Sqrt(
            Mathf.Pow(a.data.X - b.data.X, 2) +
            Mathf.Pow(a.data.Y - b.data.Y, 2)
        );

    }

    void ReconstructPath(
        Dictionary<PathNode<Tile>, PathNode<Tile>> CameFrom,
        PathNode<Tile> current
    )
    {
        // So at this point, current IS the goal.
        // So what we want to do is walk backwards through the Came_From
        // map, until we reach the "end" of that map...which will be
        // our starting node!
        Queue<Tile> TotalPath = new Queue<Tile>();
        TotalPath.Enqueue(current.data); // This "final" step is the path is the goal!

        while (CameFrom.ContainsKey(current))
        {
            // Came_From is a map, where the
            //    key => value relation is real saying
            //    some_node => we_got_there_from_this_node

            current = CameFrom[current];
            TotalPath.Enqueue(current.data);
        }

        // At this point, total_path is a queue that is running
        // backwards from the END tile to the START tile, so let's reverse it.

        path = new Queue<Tile>(TotalPath.Reverse());

    }

    public Tile Dequeue()
    {
        return path.Dequeue();
    }

    public int Length()
    {
        if (path == null)
            return 0;

        return path.Count;
    }

}
