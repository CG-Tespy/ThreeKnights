using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains details about a tile-moving that happened.
/// </summary>
public class TileMoveArgs : System.EventArgs
{
    protected List<TileController> tilesInvolved =          new List<TileController>();

    public List<TileController> TilesInvolved
    {
        get { return tilesInvolved; }
    }
}
