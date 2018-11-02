using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class TileSpriteController : MonoBehaviour
{

    // The only tile sprite we have right now, so this
    // it a pretty simple way to handle it.
    public Sprite FloorSprite;  // FIXME!
    public Sprite EmptySprite;  // FIXME!

    Dictionary<Tile, GameObject> TileGameObjectMap;

    World World
    {
        get { return WorldController.Instance.World; }
    }

    // Use this for initialization
    void Start()
    {
        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        TileGameObjectMap = new Dictionary<Tile, GameObject>();

        // Create a GameObject for each of our tiles, so they show visually. (and redunt reduntantly)
        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                // Get the tile data
                Tile TileData = World.GetTileAt(x, y);

                // This creates a new GameObject and adds it to our scene.
                GameObject TileGo = new GameObject();

                // Add our tile/GO pair to the dictionary.
                TileGameObjectMap.Add(TileData, TileGo);

                TileGo.name = "Tile_" + x + "_" + y;
                TileGo.transform.position = new Vector3(TileData.X, TileData.Y, 0);
                TileGo.transform.SetParent(this.transform, true);

                // Add a Sprite Renderer
                // Add a default sprite for empty tiles.
                SpriteRenderer SR = TileGo.AddComponent<SpriteRenderer>();
                SR.sprite = EmptySprite;
                SR.sortingLayerName = "Tiles";
                //if ( != )
                SR.material = Resources.Load<Material>("Materials/FloorMaterial");

                OnTileChanged(TileData);
            }
        }

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        World.RegisterTileChanged(OnTileChanged);
    }

    // THIS IS AN EXAMPLE -- NOT CURRENTLY USED (and probably out of date)
    void DestroyAllTileGameObjects()
    {
        // This function might get called when we are changing floors/levels.
        // We need to destroy all visual **GameObjects** -- but not the actual tile data!

        while (TileGameObjectMap.Count > 0)
        {
            Tile tileData = TileGameObjectMap.Keys.First();
            GameObject TileGo = TileGameObjectMap[tileData];

            // Remove the pair from the map
            TileGameObjectMap.Remove(tileData);

            // Unregister the callback!
            tileData.UnregisterTileTypeChangedCallback(OnTileChanged);

            // Destroy the visual GameObject
            Destroy(TileGo);
        }

        // Presumably, after this function gets called, we'd be calling another
        // function to build all the GameObjects for the tiles on the new floor/level
    }

    // This function should be called automatically whenever a tile's data gets changed.
    void OnTileChanged(Tile TileData)
    {

        if (TileGameObjectMap.ContainsKey(TileData) == false)
        {
            //Debug.LogError("tileGameObjectMap doesn't contain the tile_data -- did you forget to add the tile to the dictionary? Or maybe forget to unregister a callback?");
            return;
        }

        GameObject TileGo = TileGameObjectMap[TileData];

        if (TileGo == null)
        {
            //Debug.LogError("tileGameObjectMap's returned GameObject is null -- did you forget to add the tile to the dictionary? Or maybe forget to unregister a callback?");
            return;
        }

        if (TileData.Type == TileType.Floor)
        {
            TileGo.GetComponent<SpriteRenderer>().sprite = FloorSprite;
        }
        else if (TileData.Type == TileType.Empty)
        {
            TileGo.GetComponent<SpriteRenderer>().sprite = EmptySprite;
        }
        else
        {
            //Debug.LogError("OnTileTypeChanged - Unrecognized tile type.");
        }


    }



}
