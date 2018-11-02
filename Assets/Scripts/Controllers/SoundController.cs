using UnityEngine;
using System.Collections;

public class SoundController : MonoBehaviour
{
    float SoundCooldown = 0;

    // Use this for initialization
    void Start()
    {
        WorldController.Instance.World.RegisterFurnitureCreated(OnFurnitureCreated);

        WorldController.Instance.World.RegisterTileChanged(OnTileChanged);
    }

    // Update is called once per frame
    void Update()
    {
        SoundCooldown = SoundCooldown - Time.deltaTime;
    }

    void OnTileChanged(Tile tileData)
    {
        // FIXME

        if (SoundCooldown > 0)
            return;

        AudioClip ac = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        SoundCooldown = 0.1f;
    }

    public void OnFurnitureCreated(Furniture furn)
    {
        // FIXME
        if (SoundCooldown > 0)
            return;

        AudioClip ac = Resources.Load<AudioClip>("Sounds/" + furn.objectType + "_OnCreated");

        if (ac == null)
        {
            // WTF?  What do we do?
            // Since there's no specific sound for whatever Furniture this is, just
            // use a default sound -- i.e. the Wall_OnCreated sound.
            ac = Resources.Load<AudioClip>("Sounds/Wall_OnCreated");
        }

        AudioSource.PlayClipAtPoint(ac, Camera.main.transform.position);
        SoundCooldown = 0.1f;
    }
}
