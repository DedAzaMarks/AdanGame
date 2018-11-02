using UnityEngine;

public enum BuildMode
{
    FLOOR,
    FURNITURE,
    DECONSTRUCT
}
public class BuildModeController : MonoBehaviour
{
    public BuildMode BuildMode = BuildMode.FLOOR;
    bool BuildModeIsObjects = false;
    TileType BuildModeTile = TileType.Floor;
    string BuildModeObjectType;

    // Use this for initialization
    void Start()
    {
    }

    public void SetModeBuildFloor()
    {
        BuildModeIsObjects = false;
        BuildModeTile = TileType.Floor;
    }

    public void SetModeBulldoze()
    {
        BuildModeIsObjects = false;
        BuildModeTile = TileType.Empty;
    }

    public void SetModeBuildFurniture(string objectType)
    {
        // Wall is not a Tile!  Wall is an "Furniture" that exists on TOP of a tile.
        BuildModeIsObjects = true;
        BuildModeObjectType = objectType;
    }

    public void SetModeBulldozeFurniture()
    {

        BuildMode = BuildMode.DECONSTRUCT;
        GameObject.FindObjectOfType<MouseController>().StartBuildMode();

    }

    public void DoPathfindingTest()
    {
        WorldController.Instance.World.SetupPathfindingExample();

        PathTileGraph tileGraph = new PathTileGraph(WorldController.Instance.World);
    }

    public void DoBuild(Tile t)
    {
        if (BuildModeIsObjects == true)
        {
            // Create the Furniture and assign it to the tile

            // FIXME: This instantly builds the furnite:
            //WorldController.Instance.World.PlaceFurniture( buildModeObjectType, t );

            // Can we build the furniture in the selected tile?
            // Run the ValidPlacement function!

            string furnitureType = BuildModeObjectType;

            if (
                WorldController.Instance.World.IsFurniturePlacementValid(furnitureType, t) &&
                t.PendingFurnitureJob == null
            )
            {
                // This tile position is valid for this furniture
                // Create a job for it to be build

                Job j = new Job(t, furnitureType, (theJob) =>
                {
                    WorldController.Instance.World.PlaceFurniture(furnitureType, theJob.Tile);

                    // FIXME: I don't like having to manually and explicitly set
                    // flags that preven conflicts. It's too easy to forget to set/clear them!
                    t.PendingFurnitureJob = null;
                }
                );


                // FIXME: I don't like having to manually and explicitly set
                // flags that preven conflicts. It's too easy to forget to set/clear them!
                t.PendingFurnitureJob = j;
                j.RegisterJobCancelCallback((theJob) => { theJob.Tile.PendingFurnitureJob = null; });

                // Add the job to the queue
                WorldController.Instance.World.MyJobQueue.Enqueue(j);
            }
        }
        else
        {
            // We are in tile-changing mode.
            t.Type = BuildModeTile;
        }
        if (BuildMode == BuildMode.DECONSTRUCT)
        {
            // TODO
            if (t.Furniture != null)
            {
                t.Furniture.Deconstruct();
            }

        }

    }

}
