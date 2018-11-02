using UnityEngine;
using System.Collections.Generic;

public class CharacterSpriteController : MonoBehaviour
{

    Dictionary<Character, GameObject> CharacterGameObjectMap;

    Dictionary<string, Sprite> CharacterSprites;

    World World
    {
        get { return WorldController.Instance.World; }
    }

    // Use this for initialization
    void Start()
    {
        LoadSprites();

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        CharacterGameObjectMap = new Dictionary<Character, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        World.RegisterCharacterCreated(OnCharacterCreated);

        // Check for pre-existing characters, which won't do the callback.
        foreach (Character c in World.Characters)
        {
            OnCharacterCreated(c);
        }


        //c.SetDestination( world.GetTileAt( world.Width/2 + 5, world.Height/2 ) );
    }

    void LoadSprites()
    {
        CharacterSprites = new Dictionary<string, Sprite>();
        Sprite[] Sprites = Resources.LoadAll<Sprite>("Images/Characters/");

        //Debug.Log("LOADED RESOURCE:");
        foreach (Sprite s in Sprites)
        {
            //Debug.Log(s);
            CharacterSprites[s.name] = s;
        }
    }

    public void OnCharacterCreated(Character c)
    {
        Debug.Log("OnCharacterCreated");
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject CharGo = new GameObject();


        //IMPORTANT ONE
        // Material material = GetComponent<Renderer>().material;
        // char_go.GetComponent<Material>();
        //material = char_go.AddComponent<Material>();

        // Add our tile/GO pair to the dictionary.
        CharacterGameObjectMap.Add(c, CharGo);

        CharGo.name = "Character";
        CharGo.transform.position = new Vector3(c.X, c.Y, 0);
        

        SpriteRenderer SR = CharGo.AddComponent<SpriteRenderer>();
        SR.sprite = CharacterSprites["p1_front"];
        SR.sortingLayerName = "Characters";

        // Material material = char_go.GetComponent<Renderer>().material;
        // material.shader = Shader.Find("Diffuse");

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        c.RegisterOnChangedCallback(OnCharacterChanged);

    }

    void OnCharacterChanged(Character c)
    {
        //Debug.Log("OnFurnitureChanged");
        // Make sure the furniture's graphics are correct.

        if (CharacterGameObjectMap.ContainsKey(c) == false)
        {
            //Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map.");
            return;
        }

        GameObject CharGo = CharacterGameObjectMap[c];
        //Debug.Log(furn_go);
        //Debug.Log(furn_go.GetComponent<SpriteRenderer>());

        //char_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

        CharGo.transform.position = new Vector3(c.X, c.Y, 0);
    }



}
