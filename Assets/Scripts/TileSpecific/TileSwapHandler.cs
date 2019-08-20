using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TileSwapHandler : MonoBehaviour
{
    [SerializeField] TileType airTileType;
    TileBoardController tileBoard;
    public TileController firstTileClicked, secondTileClicked;
    TileSwapArgs swapResults =                      new TileSwapArgs();
    public static UnityAction<TileSwapHandler, TileSwapArgs> AnySwapMade =  delegate {};
    // ^ So custom code can respond to the swaps without having to be involved in a Fungus block.

    void Awake()
    {
        tileBoard =                                 FindObjectOfType<TileBoardController>();
        TileController.AnyClicked +=                OnAnyTileClicked;
    }

    void OnDestroy()
    {
        // Always make sure to clean up when necessary, if listening to static events!
        TileController.AnyClicked -=                OnAnyTileClicked;
    }

    void OnAnyTileClicked(TileController tile)
    {
        if (firstTileClicked == null)
            firstTileClicked =                  tile;
        else if (secondTileClicked == null && firstTileClicked != tile)
            secondTileClicked =                 tile;

        if (secondTileClicked != null) // Consider swapping the tiles.
        {
            if (CanSwapTiles(firstTileClicked, secondTileClicked))
            {
                SwapTiles(firstTileClicked, secondTileClicked);
                UpdateSwapResults();

                // Alert listeners, be it Fungus blocks or custom code.
                AnySwapMade.Invoke(this, swapResults);
                SwapMadeEvent.Invoke(swapResults);
            }

            // For the next swap.
            UnregisterTileClicks();
        }

    }

    public void SwapTiles(TileController firstTile, TileController secondTile)
    {
        // Simply swap the world and grid positions of the tiles. Maybe in the future, 
        // we can make the swap more aesthetically-pleasing, but for now, this'll do.
        Vector3 cachedWorldPos =                firstTile.transform.position;
        Vector2Int cachedBoardPos =             firstTile.BoardPos;

        firstTile.transform.position =          secondTile.transform.position;
        firstTile.BoardPos =                    secondTile.BoardPos;

        secondTile.transform.position =         cachedWorldPos;
        secondTile.BoardPos =                   cachedBoardPos;

    }

    void UnregisterTileClicks()
    {
        firstTileClicked =                          null;
        secondTileClicked =                         null;
    }

    bool CanSwapTiles(TileController tile1, TileController tile2)
    {
        // By default, the tiles can be swapped if they're just one space away from each other. We'll add the 
        // knight jump functionality later down the line.
        float tileDist =                            Vector2Int.Distance(tile1.BoardPos, tile2.BoardPos);
        bool nearEnough =                           tileDist <= 1;

        // Of course, we have rules to enforce... like how horizontal swaps with air tiles aren't
        // allowed.
        bool horizSwap =                            tile1.BoardPos.x != tile2.BoardPos.x;
        if (horizSwap)
        {
            if (tile1.Type == airTileType || tile2.Type == airTileType)
                return false;
        }

        // Or vertical swaps where the air tile was higher than the non-air tile.
        bool vertSwap =                             tile1.BoardPos.y != tile2.BoardPos.y;
        if (vertSwap)
        {
            TileController higherTile =             null;
            TileController lowerTile =              null;

            if (tile1.BoardPos.y > tile2.BoardPos.y)
            {
                higherTile =                        tile1;
                lowerTile =                         tile2;
            }
            else
            {
                higherTile =                        tile2;
                lowerTile =                         tile1;
            }

            if (higherTile.Type == airTileType)
                return false;

        }

        return nearEnough;
    }


    void UpdateSwapResults()
    {
        List<TileController> tiles =                swapResults.TilesInvolved;
        tiles.Clear();
        tiles.Add(firstTileClicked);
        tiles.Add(secondTileClicked);
    }
    
}
