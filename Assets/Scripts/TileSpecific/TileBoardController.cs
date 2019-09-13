using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fungus;
using System;

/// <summary>
/// Controls the tile board, managing various aspects of it.
/// </summary>
public class TileBoardController : MonoBehaviour
{
    [Tooltip("Flowchart that contains values pertaining to the tile board.")]
    [SerializeField] Flowchart tileBoardVals;
    [SerializeField] TileType airTileType;
    [SerializeField] protected Transform tileHolder;

    IntegerVariable minAmountForMatch;
    [SerializeField] BoardGenerator boardGenerator;
    #region Tiles
    List<TileController> tiles =                        new List<TileController>();

    public List<TileController> Tiles
    {
        get                                             { return tiles; }
    }

    List<List<TileController>> tileRows =               new List<List<TileController>>();
    List<List<TileController>> tileColumns =            new List<List<TileController>>();
    #endregion

    #region For event listeners
    MatchArgs matchesOnBoard =                          new MatchArgs();
    TileClearArgs tileClearReport =                     new TileClearArgs();
    #endregion
    
    public int RowCount                                 { get { return tileRows.Count; } }
    public int ColumnCount                              { get { return tileColumns.Count; } }

    void Awake()
    {
        TileSwapHandler.AnySwapMade +=                  OnAnySwapMade;
        minAmountForMatch =                             tileBoardVals.GetVariable("minAmountForMatch") 
                                                        as IntegerVariable;
    }

    void Start()
    {
        // The tiles should be made in Awake by the board generator
        RegisterTiles();
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
    
    public IList<TileController> GetTilesRelativeTo(TileController tile, int howManyTiles, Vector2Int direction)
    {
        List<TileController> tiles = new List<TileController>();
        Vector2Int tilePosition = tile.BoardPos;
        for (int i = 1; i < howManyTiles + 1; i++)
        {   
            tilePosition += new Vector2Int(Math.Sign(direction.x) * i, Math.Sign(direction.y) * i);
            TileController relativeTile = GetTileAt(tilePosition);
            if (relativeTile != null)
                tiles.Add(relativeTile);
        }

        return tiles;
    }

    void OnAnySwapMade(TileSwapHandler swapHandler, TileSwapArgs swapArgs)
    {
        UpdateColumnsAndRows();
        UpdateMatchesOnBoard();

        // Whatever tiles make up a match should be turned into air tiles. So do that,
        // while alerting listeners of it.
        foreach (TileController tile in matchesOnBoard.TilesMatched)
        {
            if (tile.Type == airTileType) // Avoid counting the same tile twice, in case of a T-match or somesuch
                continue;
            tileClearReport.TileCleared =           tile;
            tileClearReport.OriginalTileType =      tile.Type;
            tile.Type =                             airTileType;
            TileClearedEvent.Invoke(tileClearReport);
        }

        // With the matched tiles now aired up, their matches don't count
        // anymore.
        matchesOnBoard.TilesMatched.Clear();

        List<TileController> tilesSwapped =         swapArgs.TilesInvolved;
    }

    void RegisterTiles()
    {
        tiles.AddRange(tileHolder.GetComponentsInChildren<TileController>());
        UpdateColumnsAndRows();
    }

    #region Helpers
    /// <summary>
    /// Scans the board for any matches, and updates the MatchArgs object 
    /// keeping track of the matches currently on the board.
    /// </summary>
    void UpdateMatchesOnBoard()
    {
        List<TileController> tilesMatched =         matchesOnBoard.TilesMatched;

        foreach (List<TileController> row in tileRows)
        {
            tilesMatched.AddRange(MatchedTilesInList(row));
        }

        foreach (List<TileController> column in tileColumns)
        {
            tilesMatched.AddRange(MatchedTilesInList(column));
        }

    }

    /// <summary>
    /// Sets the column and row lists based on the coordinates of the tiles
    /// </summary>
    void UpdateColumnsAndRows()
    {
        tileColumns.Clear();
        tileRows.Clear();

        // Make sure the lists are sorted based on their coords
        for (int x = 0; x < boardGenerator.numOfColumns; x++)
        {
            List<TileController> tileColumn =       (from tile in Tiles
                                                    where tile.BoardPos.x == x
                                                    orderby tile.BoardPos.y
                                                    select tile).ToList();
            tileColumns.Add(tileColumn);
        }

        for (int y = 0; y < boardGenerator.numOfRows; y++)
        {
            List<TileController> rowColumn =        (from tile in Tiles
                                                    where tile.BoardPos.y == y
                                                    orderby tile.BoardPos.x
                                                    select tile).ToList();
            tileRows.Add(rowColumn);
        }
    }

    List<TileController> MatchedTilesInList(List<TileController> toCheck)
    {
        List<TileController> matchedTiles =         new List<TileController>();
        List<TileController> tileChain =            new List<TileController>();
        bool enoughForMatch =                       false;

        for (int i = 0; i < toCheck.Count; i++)
        {
            TileController tile =                   toCheck[i];
            TileController previousTile =           null;

            bool thereIsPreviousTile =              i > 0;

            if (thereIsPreviousTile)
            {
                previousTile =                      toCheck[i - 1];

                // Air tiles don't count in matches.
                bool bothTilesChain =               tile.Type != airTileType && tile.Type == previousTile.Type;
                enoughForMatch =                    tileChain.Count >= minAmountForMatch.Value;

                if (bothTilesChain)
                {
                    tileChain.Add(tile);
                }
                else
                {
                    if (enoughForMatch)
                    {
                        // Register the match
                        matchedTiles.AddRange(tileChain);
                    }

                    // Reset our chain, having it start with the latest tile
                    tileChain.Clear();
                    tileChain.Add(tile); // New start of a chain
                }
                
            }
            else // Let this first tile be the start of a chain
            {
                tileChain.Add(tile);
            }

        }

        if (tileChain.Count >= minAmountForMatch.Value)
            matchedTiles.AddRange(tileChain);
        
        return matchedTiles;
    }

    #endregion
}
