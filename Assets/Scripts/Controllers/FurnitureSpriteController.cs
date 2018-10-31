using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class FurnitureSpriteController : MonoBehaviour
{

    Dictionary<Furniture, GameObject> furnitureGameObjectMap;

    Dictionary<string, Sprite> furnitureSprites;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    // Use this for initialization
    void Start()
    {
        LoadSprites();

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.RegisterFurnitureCreated(OnFurnitureCreated);

        // Go through any EXISTING furniture (i.e. from a save that was loaded OnEnable) and call the OnCreated event manually
        foreach (Furniture furn in world.furnitures)
        {
            OnFurnitureCreated(furn);
        }
    }

    void LoadSprites()
    {
        furnitureSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Furniture/");

        //Debug.Log("LOADED RESOURCE:");
        foreach (Sprite s in sprites)
        {
            //Debug.Log(s);
            furnitureSprites[s.name] = s;
        }
    }



    public void OnFurnitureCreated(Furniture furn)
    {
        //Debug.Log("OnFurnitureCreated");
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject furn_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        furnitureGameObjectMap.Add(furn, furn_go);

        if (furn.objectType == "Lamp")
        {
            Light light = furn_go.AddComponent<Light>();
            light.color = Color.white;
            //light.
            furn_go.transform.position = new Vector3(furn.tile.X, furn.tile.Y, -1.0f);
            //Debug.Log("Lamp coord " + furn_go.transform.position.x + " " + furn_go.transform.position.y + " " + furn_go.transform.position.z);
        }


        furn_go.name = furn.objectType + "_" + furn.tile.X + "_" + furn.tile.Y;
        if (furn.objectType != "Lamp")
            furn_go.transform.position = new Vector3(furn.tile.X, furn.tile.Y, 0);
        furn_go.transform.SetParent(this.transform, true);

        // FIXME: This hardcoding is not ideal!
        if (furn.objectType == "Door")
        {
            // By default, the door graphic is meant for walls to the east & west
            // Check to see if we actually have a wall north/south, and if so
            // then rotate this GO by 90 degrees



            Tile northTile = world.GetTileAt(furn.tile.X, furn.tile.Y + 1);
            Tile southTile = world.GetTileAt(furn.tile.X, furn.tile.Y - 1);

            if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null &&
                northTile.furniture.objectType == "Wall" && southTile.furniture.objectType == "Wall")
            {
                furn_go.transform.rotation = Quaternion.Euler(0, 0, 90);
                furn_go.transform.Translate(1f, 0, 0, Space.World); // UGLY HACK TO COMPENSATE FOR BOTTOM_LEFT ANCHOR POINT!


            }
        }



        SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFurniture(furn);
        sr.sortingLayerName = "Furniture";
        if (furn.objectType != "Lamp")
            sr.material = Resources.Load<Material>("Materials/FurnitureMaterial");

        // Material material = furn_go.GetComponent<Renderer>().material;
        // material = Resources.Load<Material>("Materials/MyMaterial");
        // furn_go.material = Material;
        // material.shader = Shader.Find("Diffuse");
        // material.color = Color.white;

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        furn.RegisterOnChangedCallback(OnFurnitureChanged);
        furn.RegisterOnRemovedCallback(OnFurnitureRemoved);

    }

    void OnFurnitureRemoved(Furniture furniture)
    {
        if (furnitureGameObjectMap.ContainsKey(furniture) == false)
        {
            Debug.LogError("OnFurnitureRemoved -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furniture];
        Destroy(furn_go);
        furnitureGameObjectMap.Remove(furniture);
    }

    void OnFurnitureChanged(Furniture furn)
    {
        //Debug.Log("OnFurnitureChanged");
        // Make sure the furniture's graphics are correct.

        if (furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.LogError("OnFurnitureChanged -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];
        //Debug.Log(furn_go);
        //Debug.Log(furn_go.GetComponent<SpriteRenderer>());

        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

    }




    public Sprite GetSpriteForFurniture(Furniture furn)
    {
        string spriteName = furn.objectType;

        if (furn.linksToNeighbour == false)
        {

            // If this is a DOOR, let's check OPENNESS and update the sprite.
            // FIXME: All this hardcoding needs to be generalized later.
            if (furn.objectType == "Door")
            {
                if (furn.furnParameters["openness"] < 0.1f)
                {
                    // Door is closed
                    spriteName = "Door";
                }
                else if (furn.furnParameters["openness"] < 0.5f)
                {
                    // Door is a bit open
                    spriteName = "Door_openness_1";
                }
                else if (furn.furnParameters["openness"] < 0.9f)
                {
                    // Door is a lot open
                    spriteName = "Door_openness_2";
                }
                else
                {
                    // Door is a fully open
                    spriteName = "Door_openness_3";
                }
                //Debug.Log(spriteName);
            }



            return furnitureSprites[spriteName];
        }

        // Otherwise, the sprite name is more complicated.

        spriteName = furn.objectType + "_";

        // Check for neighbours North, East, South, West

        int x = furn.tile.X;
        int y = furn.tile.Y;

        Tile t;

        t = world.GetTileAt(x, y + 1);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "N";
        }
        t = world.GetTileAt(x + 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "E";
        }
        t = world.GetTileAt(x, y - 1);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "S";
        }
        t = world.GetTileAt(x - 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "W";
        }

        // For example, if this object has all four neighbours of
        // the same type, then the string will look like:
        //       Wall_NESW

        if (furnitureSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("GetSpriteForInstalledObject -- No sprites with name: " + spriteName);
            return null;
        }


        return furnitureSprites[spriteName];

    }


    public Sprite GetSpriteForFurniture(string objectType)
    {
        if (furnitureSprites.ContainsKey(objectType))
        {
            return furnitureSprites[objectType];
        }

        if (furnitureSprites.ContainsKey(objectType + "_"))
        {
            return furnitureSprites[objectType + "_"];
        }

        Debug.LogError("GetSpriteForFurniture -- No sprites with name: " + objectType);
        return null;
    }
}
