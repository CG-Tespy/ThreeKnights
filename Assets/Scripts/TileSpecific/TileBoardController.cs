using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the tile board.
/// </summary>
public class TileBoardController : MonoBehaviour
{
    [SerializeField] TileType airTileType;
    [SerializeField] protected Transform tileHolder;
    List<TileController> tiles =                new List<TileController>();

    public List<TileController> Tiles
    {
        get                                     { return tiles; }
    }

    void Awake()
    {
        TileSwapHandler.AnySwapMade +=                  OnAnySwapMade;
    }

    void Start()
    {
        tiles.AddRange(tileHolder.GetComponentsInChildren<TileController>());
    }

    void OnDestroy()
    {
        TileSwapHandler.AnySwapMade -=                  OnAnySwapMade;
    }

    public TileController GetTileAt(Vector2Int position)
    {
        foreach (TileController tile in tiles)
        {
            if (tile.BoardPos.Equals(position))
                return tile;
        }

        return null;
    }

    public TileController GetTileAt(int xPosition, int yPosition)
    {
        return GetTileAt(new Vector2Int(xPosition, yPosition));
    }

    void OnAnySwapMade(TileSwapHandler swapHandler, TileSwapArgs swapArgs)
    {
        List<TileController> tilesSwapped =         swapArgs.TilesInvolved;

        if (ContainsAirTile(tilesSwapped))
            return; // Just let the swap be
        else
            TurnIntoAirTiles(tilesSwapped);
    }


    bool ContainsAirTile(ICollection<TileController> tiles)
    {
        foreach (TileController tile in tiles)
            if (tile.Type == airTileType)
                return true;

        return false;
    }

    void TurnIntoAirTiles(ICollection<TileController> tiles)
    {
        foreach (TileController tile in tiles)
            tile.Type =                             airTileType;
    }
}
