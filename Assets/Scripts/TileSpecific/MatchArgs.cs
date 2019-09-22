using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains details about a tile-match.
/// </summary>
public class MatchArgs : System.EventArgs
{
    protected List<TileController> tilesMatched = new List<TileController>();
    public List<TileController> TilesMatched
    {
        get { return tilesMatched; }
    }
    public int MatchCount
    {
        get { return tilesMatched.Count; }
    }
}
