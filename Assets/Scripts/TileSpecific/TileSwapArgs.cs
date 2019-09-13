using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains details about a tile-swapping that happened or may happen.
/// </summary>
public class TileSwapArgs : System.EventArgs
{
    protected List<TileController> tilesInvolved =          new List<TileController>();
    protected TileSwapType swapType;
    protected float moveDuration;
    public List<TileController> TilesInvolved
    {
        get                                                 { return tilesInvolved; }
    }
    public TileSwapType SwapType
    {
        get                                                 { return swapType; }
        set                                                 { swapType = value; }
    }

    public float MoveDuration
    {
        get                                                 { return moveDuration; }
        set                                                 { moveDuration = value; }
    }
}
