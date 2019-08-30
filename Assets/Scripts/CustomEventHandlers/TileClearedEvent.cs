using UnityEngine;
using Fungus;

[EventHandlerInfo("Three Knights",
                      "Tile Cleared",
                      "The block will execute when a tile is cleared.")]
    [AddComponentMenu("")]
public class TileClearedEvent : SimpleEvent<TileClearedEvent, TileClearArgs> {}

// Details related to a tile being cleared in the board.
public class TileClearArgs
{
    TileController tileCleared;
    TileType originalTileType;

    public TileController TileCleared 
    { 
        get                                 { return tileCleared; }
        set                                 { tileCleared = value; } 
    }

    public TileType OriginalTileType
    {
        get                                 { return originalTileType; }
        set                                 { originalTileType = value; }
    }
}