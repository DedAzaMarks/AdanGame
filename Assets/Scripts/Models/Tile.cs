
using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


// TileType is the base type of the tile. In some tile-based games, that might be
// the terrain type. For us, we only need to differentiate between empty space
// and floor (a.k.a. the station structure/scaffold). Walls/Doors/etc... will be
// InstalledObjects sitting on top of the floor.
public enum TileType { Empty, Floor };

public enum ENTERABILITY { Yes, Never, Soon };

public class Tile : IXmlSerializable
{
    private TileType _type = TileType.Empty;
    public TileType Type
    {
        get { return _type; }
        set
        {
            TileType oldType = _type;
            _type = value;
            // Call the callback and let things know we've changed.

            if (cbTileChanged != null && oldType != _type)
            {
                cbTileChanged(this);
            }
        }
    }

    // LooseObject is something like a drill or a stack of metal sitting on the floor
    Inventory inventory;

    // Furniture is something like a wall, door, or sofa.
    public Furniture furniture
    {
        get; protected set;
    }

    // FIXME: This seems like a terrible way to flag if a job is pending
    // on a tile.  This is going to be prone to errors in set/clear.
    public Job pendingFurnitureJob;

    // We need to know the context in which we exist. Probably. Maybe.
    public World world { get; protected set; }

    public int X { get; protected set; }
    public int Y { get; protected set; }

    public float movementCost
    {
        get
        {

            if (Type == TileType.Empty)
                return 0;   // 0 is unwalkable

            if (furniture == null)
                return 1;

            return 1 * furniture.movementCost;
        }
    }

    // The function we callback any time our tile's data changes
    Action<Tile> cbTileChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"/> class.
    /// </summary>
    /// <param name="world">A World instance.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile(World world, int x, int y)
    {
        this.world = world;
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    /// Register a function to be called back when our tile type changes.
    /// </summary>
    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged += callback;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged -= callback;
    }

    public bool PlaceFurniture(Furniture objInstance)
    {
        if (objInstance == null)
        {
            // We are uninstalling whatever was here before.
            furniture = null;
            return true;
        }

        // objInstance isn't null

        if (furniture != null)
        {
            Debug.LogError("Trying to assign a furniture to a tile that already has one!");
            return false;
        }

        // At this point, everything's fine!

        furniture = objInstance;
        return true;
    }

    // Tells us if two tiles are adjacent.
    public bool IsNeighbour(Tile tile, bool diagOkay = false)
    {
        // Check to see if we have a difference of exactly ONE between the two
        // tile coordinates.  Is so, then we are vertical or horizontal neighbours.
        return
            Mathf.Abs(this.X - tile.X) + Mathf.Abs(this.Y - tile.Y) == 1 ||  // Check hori/vert adjacency
            (diagOkay && (Mathf.Abs(this.X - tile.X) == 1 && Mathf.Abs(this.Y - tile.Y) == 1)) // Check diag adjacency
            ;
    }

    /// <summary>
    /// Gets the neighbours.
    /// </summary>
    /// <returns>The neighbours.</returns>
    /// <param name="diagOkay">Is diagonal movement okay?.</param>
    public Tile[] GetNeighbours(bool diagOkay = false)
    {
        Tile[] ns;

        if (diagOkay == false)
        {
            ns = new Tile[4];   // Tile order: N E S W
        }
        else
        {
            ns = new Tile[8];   // Tile order : N E S W NE SE SW NW
        }

        Tile n;

        n = world.GetTileAt(X, Y + 1);
        ns[0] = n;  // Could be null, but that's okay.
        n = world.GetTileAt(X + 1, Y);
        ns[1] = n;  // Could be null, but that's okay.
        n = world.GetTileAt(X, Y - 1);
        ns[2] = n;  // Could be null, but that's okay.
        n = world.GetTileAt(X - 1, Y);
        ns[3] = n;  // Could be null, but that's okay.

        if (diagOkay == true)
        {
            n = world.GetTileAt(X + 1, Y + 1);
            ns[4] = n;  // Could be null, but that's okay.
            n = world.GetTileAt(X + 1, Y - 1);
            ns[5] = n;  // Could be null, but that's okay.
            n = world.GetTileAt(X - 1, Y - 1);
            ns[6] = n;  // Could be null, but that's okay.
            n = world.GetTileAt(X - 1, Y + 1);
            ns[7] = n;  // Could be null, but that's okay.
        }

        return ns;
    }


    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader)
    {
        Type = (TileType)int.Parse(reader.GetAttribute("Type"));
    }

    public ENTERABILITY IsEnterable()
    {
        // This returns true if you can enter this tile right this moment.
        if (movementCost == 0)
            return ENTERABILITY.Never;

        // Check out furniture to see if it has a special block on enterability
        if (furniture != null && furniture.IsEnterable != null)
        {
            return furniture.IsEnterable(furniture);
        }

        return ENTERABILITY.Yes;
    }
}
