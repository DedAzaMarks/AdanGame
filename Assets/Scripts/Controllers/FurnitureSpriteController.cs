using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class FurnitureSpriteController : MonoBehaviour
{

    Dictionary<Furniture, GameObject> FurnitureGameObjectMap;

    Dictionary<string, Sprite> FurnitureSprites;

    World World
    {
        get { return WorldController.Instance.World; }
    }

    // Use this for initialization
    void Start()
    {
        LoadSprites();

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        FurnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        World.RegisterFurnitureCreated(OnFurnitureCreated);

        // Go through any EXISTING furniture (i.e. from a save that was loaded OnEnable) and call the OnCreated event manually
        foreach (Furniture furn in World.Furnitures)
        {
            OnFurnitureCreated(furn);
        }
    }

    void LoadSprites()
    {
        FurnitureSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Furniture/");

        //Debug.Log("LOADED RESOURCE:");
        foreach (Sprite s in sprites)
        {
            //Debug.Log(s);
            FurnitureSprites[s.name] = s;
        }
    }



    public void OnFurnitureCreated(Furniture Furn)
    {
        //Debug.Log("OnFurnitureCreated");
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject FurnGo = new GameObject();

        // Add our tile/GO pair to the dictionary.
        FurnitureGameObjectMap.Add(Furn, FurnGo);

        if (Furn.objectType == "Lamp")
        {
            Light Light = FurnGo.AddComponent<Light>();
            Light.color = Color.white;
            //light.
            FurnGo.transform.position = new Vector3(Furn.tile.X, Furn.tile.Y, -1.0f);
            //Debug.Log("Lamp coord " + furn_go.transform.position.x + " " + furn_go.transform.position.y + " " + furn_go.transform.position.z);
        }


        FurnGo.name = Furn.objectType + "_" + Furn.tile.X + "_" + Furn.tile.Y;
        if (Furn.objectType != "Lamp")
            FurnGo.transform.position = new Vector3(Furn.tile.X, Furn.tile.Y, 0);
        FurnGo.transform.SetParent(this.transform, true);

        // FIXME: This hardcoding is not ideal!
        if (Furn.objectType == "Door")
        {
            // By default, the door graphic is meant for walls to the east & west
            // Check to see if we actually have a wall north/south, and if so
            // then rotate this GO by 90 degrees



            Tile NorthTile = World.GetTileAt(Furn.tile.X, Furn.tile.Y + 1);
            Tile SouthTile = World.GetTileAt(Furn.tile.X, Furn.tile.Y - 1);

            if (NorthTile != null && SouthTile != null && NorthTile.Furniture != null && SouthTile.Furniture != null &&
                NorthTile.Furniture.objectType == "Wall" && SouthTile.Furniture.objectType == "Wall")
            {
                FurnGo.transform.rotation = Quaternion.Euler(0, 0, 90);
                FurnGo.transform.Translate(1f, 0, 0, Space.World); // UGLY HACK TO COMPENSATE FOR BOTTOM_LEFT ANCHOR POINT!


            }
        }



        SpriteRenderer SR = FurnGo.AddComponent<SpriteRenderer>();
        SR.sprite = GetSpriteForFurniture(Furn);
        SR.sortingLayerName = "Furniture";
        if (Furn.objectType != "Lamp")
            SR.material = Resources.Load<Material>("Materials/FurnitureMaterial");

        // Material material = furn_go.GetComponent<Renderer>().material;
        // material = Resources.Load<Material>("Materials/MyMaterial");
        // furn_go.material = Material;
        // material.shader = Shader.Find("Diffuse");
        // material.color = Color.white;

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        Furn.RegisterOnChangedCallback(OnFurnitureChanged);
        Furn.RegisterOnRemovedCallback(OnFurnitureRemoved);

    }

    void OnFurnitureRemoved(Furniture furniture)
    {
        if (FurnitureGameObjectMap.ContainsKey(furniture) == false)
        {
            //Debug.LogError("OnFurnitureRemoved -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject FurnGo = FurnitureGameObjectMap[furniture];
        Destroy(FurnGo);
        FurnitureGameObjectMap.Remove(furniture);
    }

    void OnFurnitureChanged(Furniture furn)
    {
        //Debug.Log("OnFurnitureChanged");
        // Make sure the furniture's graphics are correct.

        if (FurnitureGameObjectMap.ContainsKey(furn) == false)
        {
            //Debug.LogError("OnFurnitureChanged -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject FurnGo = FurnitureGameObjectMap[furn];
        //Debug.Log(furn_go);
        //Debug.Log(furn_go.GetComponent<SpriteRenderer>());

        FurnGo.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

    }




    public Sprite GetSpriteForFurniture(Furniture furn)
    {
        string SpriteName = furn.objectType;

        if (furn.linksToNeighbour == false)
        {

            // If this is a DOOR, let's check OPENNESS and update the sprite.
            // FIXME: All this hardcoding needs to be generalized later.
            if (furn.objectType == "Door")
            {
                if (furn.furnParameters["openness"] < 0.1f)
                {
                    // Door is closed
                    SpriteName = "Door";
                }
                else if (furn.furnParameters["openness"] < 0.5f)
                {
                    // Door is a bit open
                    SpriteName = "Door_openness_1";
                }
                else if (furn.furnParameters["openness"] < 0.9f)
                {
                    // Door is a lot open
                    SpriteName = "Door_openness_2";
                }
                else
                {
                    // Door is a fully open
                    SpriteName = "Door_openness_3";
                }
                //Debug.Log(spriteName);
            }



            return FurnitureSprites[SpriteName];
        }

        // Otherwise, the sprite name is more complicated.

        SpriteName = furn.objectType + "_";

        // Check for neighbours North, East, South, West

        SpriteName = WallNeighoburs(furn, SpriteName);

        // For example, if this object has all four neighbours of
        // the same type, then the string will look like:
        //       Wall_NESW

        if (FurnitureSprites.ContainsKey(SpriteName) == false)
        {
            //Debug.LogError("GetSpriteForInstalledObject -- No sprites with name: " + spriteName);
            return null;
        }


        return FurnitureSprites[SpriteName];

    }

    string WallNeighoburs(Furniture furniture, string s)
    {
        int x = furniture.tile.X;
        int y = furniture.tile.Y;

        Tile t;

        t = World.GetTileAt(x, y + 1);
        if (FurnitureCheck(t, furniture))
        {
            s = s + "N";
        }
        t = World.GetTileAt(x + 1, y);
        if (FurnitureCheck(t, furniture))
        {
            s = s + "E";
        }
        t = World.GetTileAt(x, y - 1);
        if (FurnitureCheck(t, furniture))
        {
            s = s + "S";
        }
        t = World.GetTileAt(x - 1, y);
        if (FurnitureCheck(t, furniture))
        {
            s = s + "W";
        }

        return s;
    }

    bool FurnitureCheck(Tile t, Furniture furniture)
    {
        if (t != null && t.Furniture != null && t.Furniture.objectType == furniture.objectType)
            return true;
        return false;
    }


    public Sprite GetSpriteForFurniture(string objectType)
    {
        if (FurnitureSprites.ContainsKey(objectType))
        {
            return FurnitureSprites[objectType];
        }

        if (FurnitureSprites.ContainsKey(objectType + "_"))
        {
            return FurnitureSprites[objectType + "_"];
        }

        //Debug.LogError("GetSpriteForFurniture -- No sprites with name: " + objectType);
        return null;
    }
}
