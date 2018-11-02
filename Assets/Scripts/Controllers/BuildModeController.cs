using UnityEngine;

public enum BuildMode { DECONSTRUCT }
public class BuildModeController : MonoBehaviour
{
    public BuildMode buildMode;
    bool buildModeIsObjects = false;
    TileType buildModeTile = TileType.Floor;
    string buildModeObjectType;

    // Use this for initialization
    void Start()
    {
    }

    public void SetMode_BuildFloor()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Floor;
    }

    public void SetMode_Bulldoze()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Empty;
    }

    public void SetMode_BuildFurniture(string objectType)
    {
        // Wall is not a Tile!  Wall is an "Furniture" that exists on TOP of a tile.
        buildModeIsObjects = true;
        buildModeObjectType = objectType;
    }

    public void SetMode_BulldozeFurniture()
    {

        buildMode = BuildMode.DECONSTRUCT;
        GameObject.FindObjectOfType<MouseController>().StartBuildMode();

    }

    public void DoPathfindingTest()
    {
        WorldController.Instance.world.SetupPathfindingExample();

        Path_TileGraph tileGraph = new Path_TileGraph(WorldController.Instance.world);
    }

    public void DoBuild(Tile t)
    {
        if (buildModeIsObjects == true)
        {
            // Create the Furniture and assign it to the tile

            // FIXME: This instantly builds the furnite:
            //WorldController.Instance.World.PlaceFurniture( buildModeObjectType, t );

            // Can we build the furniture in the selected tile?
            // Run the ValidPlacement function!

            string furnitureType = buildModeObjectType;

            if (
                WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t) &&
                t.pendingFurnitureJob == null
            )
            {
                // This tile position is valid for this furniture
                // Create a job for it to be build

                Job j = new Job(t, furnitureType, (theJob) =>
                {
                    WorldController.Instance.world.PlaceFurniture(furnitureType, theJob.tile);

                    // FIXME: I don't like having to manually and explicitly set
                    // flags that preven conflicts. It's too easy to forget to set/clear them!
                    t.pendingFurnitureJob = null;
                }
                );


                // FIXME: I don't like having to manually and explicitly set
                // flags that preven conflicts. It's too easy to forget to set/clear them!
                t.pendingFurnitureJob = j;
                j.RegisterJobCancelCallback((theJob) => { theJob.tile.pendingFurnitureJob = null; });

                // Add the job to the queue
                WorldController.Instance.world.jobQueue.Enqueue(j);
            }
        }
        else
        {
            // We are in tile-changing mode.
            t.Type = buildModeTile;
        }
        if (buildMode == BuildMode.DECONSTRUCT)
        {
            // TODO
            if (t.furniture != null)
            {
                t.furniture.Deconstruct();
            }

        }

    }

}
