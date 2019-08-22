using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Controls the tile board.
/// </summary>
public class TileBoardController : MonoBehaviour
{
    [SerializeField] TileType airTileType;
    [SerializeField] protected Transform tileHolder;
    [SerializeField] protected int minAmountForMatch = 3;
    [SerializeField] BoardGenerator boardGenerator;
    List<TileController> tiles =                        new List<TileController>();

    public List<TileController> Tiles
    {
        get                                             { return tiles; }
    }

    List<List<TileController>> tileRows =               new List<List<TileController>>();
    List<List<TileController>> tileColumns =            new List<List<TileController>>();
    MatchArgs matchesOnBoard =                          new MatchArgs();
    
    public int RowCount                                 { get { return tileRows.Count; } }
    public int ColumnCount                              { get { return tileColumns.Count; } }

    void Awake()
    {
        TileSwapHandler.AnySwapMade +=                  OnAnySwapMade;
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

    void OnAnySwapMade(TileSwapHandler swapHandler, TileSwapArgs swapArgs)
    {
        UpdateMatchesOnBoard();

        // Whatever tiles make up a match should be turned into air tiles. So...
        foreach (TileController tile in matchesOnBoard.TilesMatched)
        {
            tile.Type =                             airTileType;
        }

        // With the matched tiles now aired up, their matches don't count
        // anymore.
        matchesOnBoard.TilesMatched.Clear();

        List<TileController> tilesSwapped =         swapArgs.TilesInvolved;

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

    void RegisterTiles()
    {
        tiles.AddRange(tileHolder.GetComponentsInChildren<TileController>());

        // Organize the tiles into rows and columns
        for (int x = 0; x < boardGenerator.numOfColumns; x++)
        {
            List<TileController> tileColumn =       (from tile in Tiles
                                                    where tile.BoardPos.x == x
                                                    select tile).ToList();
            tileColumns.Add(tileColumn);
        }

        for (int y = 0; y < boardGenerator.numOfRows; y++)
        {
            List<TileController> rowColumn =        (from tile in Tiles
                                                    where tile.BoardPos.y == y
                                                    select tile).ToList();
            tileRows.Add(rowColumn);
        }

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

    List<TileController> MatchedTilesInList(List<TileController> toCheck)
    {
        // TODO: Fix what's getting the wrong tiles returned
        List<TileController> matchedTiles =         new List<TileController>();
        List<TileController> tileChain =            new List<TileController>();

        for (int i = 0; i < toCheck.Count; i++)
        {
            TileController tile =               toCheck[i];
            TileController previousTile =       null;

            bool thereIsPreviousTile =          i > 0;

            if (thereIsPreviousTile)
            {
                previousTile =                  toCheck[i - 1];

                // Air tiles don't count in matches.
                bool bothTilesChain =           tile.type != airTileType && tile.type == previousTile.type;

                if (bothTilesChain)
                {
                    tileChain.Add(tile);
                }
                else if (tileChain.Count >= minAmountForMatch)
                {
                    // We have enough in our chain to count as a match. Register it, then reset it for
                    // the next chain-checking iteration.
                    matchedTiles.AddRange(tileChain);
                    tileChain.Clear();
                    tileChain.Add(tile); // New start of a chain
                } 

            }
            else // Let this first tile be the start of a chain
            {
                tileChain.Add(tile);
            }

        }
        
        return matchedTiles;
    }

    #endregion
}
