using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Fungus;

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
    TileController[,] tiles;
    List<TileController> unorderedTiles = new List<TileController>();

    public TileController[,] Tiles
    {
        get                                             { return tiles; }
    }

    int MinAmountForMatch { get { return minAmountForMatch.Value; } }

    List<List<TileController>> tileRows = new List<List<TileController>>();
    List<List<TileController>> tileColumns = new List<List<TileController>>();
    #endregion

    #region For event listeners
    MatchArgs matchesOnBoard = new MatchArgs();
    TileClearArgs tileClearReport = new TileClearArgs();
    #endregion
    
    public int RowCount                                 { get { return tileRows.Count; } }
    public int ColumnCount                              { get { return tileColumns.Count; } }

    void Awake()
    {
        TileSwapHandler.AnyPhysicalSwapMade += OnAnyPhysicalSwapMade;
        TileSwapHandler.AnyBoardSwapMade += OnAnyBoardSwapMade;
        minAmountForMatch = tileBoardVals.GetVariable("minAmountForMatch") 
                                                        as IntegerVariable;
        tiles = boardGenerator.GenerateBoard(this);
        UpdateColumnsAndRows();
    }

    void OnDestroy()
    {
        TileSwapHandler.AnyPhysicalSwapMade -= OnAnyPhysicalSwapMade;
        TileSwapHandler.AnyBoardSwapMade -= OnAnyBoardSwapMade;
    }

    public TileController GetTileAt(Vector2Int position)
    {
        return GetTileAt(position.x, position.y);
    }

    public TileController GetTileAt(int xPosition, int yPosition)
    {
        if (PositionOutOfBounds(xPosition, yPosition))
            return null;
        return tiles[xPosition, yPosition];
    }

    bool PositionOutOfBounds(int xPosition, int yPosition)
    {
        return xPosition < 0 || xPosition >= ColumnCount ||
        yPosition < 0 || yPosition >= RowCount;
    }

    public bool HasTile(TileController tile)
    {
        return GetTileAt(tile.BoardPos) != null;
    }

    void SetTileAt(TileController tile, Vector2Int boardPos)
    {
        SetTileAt(tile, boardPos.x, boardPos.y);
    }

    void SetTileAt(TileController tile, int xBoardPos, int yBoardPos)
    {
        Tiles[xBoardPos, yBoardPos] = tile;
    }

    void OnAnyPhysicalSwapMade(TileSwapHandler swapHandler, TileSwapArgs swapArgs)
    {
        if (swapArgs.SwapType != TileSwapType.freeAdjacent)
        {
            UpdateMatchesOnBoard();
            ConvertMatchedTilesToAirTiles();

            // With the matched tiles now aired up, their matches don't count
            // anymore.
            matchesOnBoard.TilesMatched.Clear();
        }
    }

    void OnAnyBoardSwapMade(TileSwapHandler swapHandler, TileSwapArgs swapArgs)
    {
        UpdateTileGridOnSwap(swapArgs.TilesInvolved);
        UpdateUnorderedTiles();
        UpdateColumnsAndRows();
    }

    void UpdateTileGridOnSwap(List<TileController> tilesSwapped)
    {
        TileController firstTile = tilesSwapped[0];
        TileController secondTile = tilesSwapped[1];

        // For when one of the tiles came here from a different board
        if (firstTile.Board == this)
            SetTileAt(firstTile, firstTile.BoardPos);
        if (secondTile.Board == this)
            SetTileAt(secondTile, secondTile.BoardPos);
    }

    void UpdateUnorderedTiles()
    {
        unorderedTiles.Clear();

        for (int x = 0; x < boardGenerator.numOfColumns; x++)
        {
            for (int y = 0; y < boardGenerator.numOfRows; y++)
            {
                TileController tile = Tiles[x, y];
                unorderedTiles.Add(tile);
            }
        }
    }

    #region Helpers
    /// <summary>
    /// Scans the board for any matches, and updates the MatchArgs object 
    /// keeping track of the matches currently on the board.
    /// </summary>
    void UpdateMatchesOnBoard()
    {
        List<TileController> tilesMatched = matchesOnBoard.TilesMatched;

        for (int i = 0; i < tileRows.Count; i++)
        {
            List<TileController> row = tileRows[i];
            tilesMatched.AddRange(MatchedTilesInList(row));
        }
        
        for (int i = 0; i < tileColumns.Count; i++)
        {
            List<TileController> column = tileColumns[i];
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

        List<TileController> column = null;
        List<TileController> row = null;

        for (int x = 0; x < boardGenerator.numOfColumns; x++)
        {
            column = (from tile in unorderedTiles
                    where tile.BoardPos.x == x
                    orderby x
                    select tile).ToList();
            tileColumns.Add(column);
        }

        for (int y = 0; y < boardGenerator.numOfRows; y++)
        {
            row = (from tile in unorderedTiles
            where tile.BoardPos.y == y
            orderby y
            select tile).ToList();

            tileRows.Add(row);
        }
    }

    void ConvertMatchedTilesToAirTiles()
    {
        // Whatever tiles make up a match should be turned into air tiles. So do that,
        // while alerting listeners of it.
        foreach (TileController tile in matchesOnBoard.TilesMatched)
        {
            if (tile.Type == airTileType) // Avoid counting the same tile twice, in case of a T-match or somesuch
                continue;

            tileClearReport.TileCleared = tile;
            tileClearReport.OriginalTileType = tile.Type;
            tile.Type = airTileType;
            TileClearedEvent.Invoke(tileClearReport);
        }
    }

    List<TileController> MatchedTilesInList(List<TileController> toCheck)
    {
        List<TileController> matchedTiles = new List<TileController>();
        List<TileController> tileChain = new List<TileController>();
        bool enoughForMatch = false;

        for (int i = 0; i < toCheck.Count; i++)
        {
            TileController tile = toCheck[i];
            TileController previousTile = null;

            bool thereIsPreviousTile = i > 0;

            if (thereIsPreviousTile)
            {
                previousTile = toCheck[i - 1];

                // Air tiles don't count in matches.
                bool bothTilesChain = tile.Type != airTileType && tile.Type == previousTile.Type;
                enoughForMatch = tileChain.Count >= MinAmountForMatch;

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
