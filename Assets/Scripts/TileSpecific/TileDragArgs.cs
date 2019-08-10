using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Details about when a tile is being dragged.
/// </summary>
public class TileDragArgs : MonoBehaviour
{
    protected TileController tileDragged;
    protected TileController otherTileTouched;

    public TileController TileDragged
    {
        get { return tileDragged; }
        set { tileDragged = value;}
    }

    public TileController OtherTileTouched
    {
        get { return otherTileTouched; }
        set { otherTileTouched = value; }
    }
}
