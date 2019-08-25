using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains details about a tile-swapping that happened.
/// </summary>
public class TileSwapArgs : System.EventArgs
{
    protected List<TileController> tilesInvolved =          new List<TileController>();
    protected TileSwapType swapType;
    public List<TileController> TilesInvolved
    {
        get                                                 { return tilesInvolved; }
    }
    public TileSwapType SwapType
    {
        get                                                 { return swapType; }
        set                                                 { swapType = value; }
    }
}
