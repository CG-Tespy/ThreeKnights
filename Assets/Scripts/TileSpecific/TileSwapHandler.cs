using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TileSwapHandler : MonoBehaviour
{
    [SerializeField] TileType airTileType;
    [Tooltip("How long, in second,  each swap should take.")]
    [SerializeField] float swapDuration =           0.5f;
    bool swapEnabled =                              true;
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
        if (!swapEnabled)
            return;
        if (firstTileClicked == null)
            firstTileClicked =                      tile;
        else if (secondTileClicked == null && firstTileClicked != tile)
            secondTileClicked =                     tile;

        if (secondTileClicked != null) // Consider swapping the tiles.
        {
            TileSwapType swapType =                 CanSwapTiles(firstTileClicked, secondTileClicked);

            if (swapType != TileSwapType.none) // We have a valid swap attempt here!
            {
                swapEnabled =                       false; // Will be reenabled once the swap is done
                StartCoroutine(SwapCoroutine(firstTileClicked, secondTileClicked, swapType));

            }

            UnregisterTileClicks();
        }

    }

    #region Helpers

    IEnumerator SwapCoroutine(TileController firstTile, TileController secondTile, TileSwapType swapType)
    {
        switch (swapType)
        {
            case TileSwapType.adjacent:
                yield return AdjacentSwapCoroutine(firstTile, secondTile);
                UpdateSwapResults(swapType);
                break;
            case TileSwapType.knight:
                yield return KnightSwapCoroutine(firstTile, secondTile);
                break;
            default:
                Debug.LogError("TileSwapType " + swapType + " not accounted for!");
                break;
        }

        swapEnabled =                               true;

    }

    IEnumerator KnightSwapCoroutine(TileController firstTile, TileController secondTile)
    {
        Vector2Int travelVec =                  secondTile.BoardPos - firstTile.BoardPos;

        // Pull off all the horizontal swaps before the vertical ones
        while (travelVec.x != 0)
        {
            // Find the appropriate adjacent tile, swap the first one with it, and update 
            // the travelVec accordingly.
            int xOffset =                       (int) Mathf.Sign(travelVec.x);
            Vector2Int otherTilePos =           firstTile.BoardPos;
            otherTilePos.x +=                   xOffset;
            TileController toSwapWith =         tileBoard.GetTileAt(otherTilePos);

            yield return AdjacentSwapCoroutine(firstTile, toSwapWith);
            travelVec.x -=                      xOffset; // Bring the value closer to 0
        }

        // Similar logic for the vertical swaps
        while (travelVec.y != 0)
        {
            int xOffset =                       (int) Mathf.Sign(travelVec.y);
            Vector2Int otherTilePos =           firstTile.BoardPos;
            otherTilePos.y +=                   xOffset;
            TileController toSwapWith =         tileBoard.GetTileAt(otherTilePos);

            yield return AdjacentSwapCoroutine(firstTile, toSwapWith);
            travelVec.y -=                      xOffset;
        }

        // Alert listeners, be it Fungus blocks or custom code.
        AnySwapMade.Invoke(this, swapResults);
        SwapMadeEvent.Invoke(swapResults);

    }

    IEnumerator AdjacentSwapCoroutine(TileController firstTile, TileController secondTile)
    {
        // Has the tiles move as they're swapped. Helps see if the swapping is even happening correctly.
        Vector3 firstPos =                          firstTile.transform.position;
        Vector3 secondPos =                         secondTile.transform.position;
        Vector2Int firstBoardPos =                  firstTile.BoardPos;

        float swapTimer =                           0f;

        while (swapTimer < swapDuration)
        {
            swapTimer +=                            Time.deltaTime;
            firstTile.transform.position =          Vector3.Lerp(firstPos, secondPos, 
                                                    swapTimer / swapDuration);
            secondTile.transform.position =         Vector3.Lerp(secondPos, firstPos, 
                                                    swapTimer / swapDuration);
            yield return new WaitForFixedUpdate();
        }

        // Update the pos on the board too, not just the world pos
        firstTile.BoardPos =                        secondTile.BoardPos;
        secondTile.BoardPos =                       firstBoardPos;

        // Alert listeners, be it Fungus blocks or custom code.
        AnySwapMade.Invoke(this, swapResults);
        SwapMadeEvent.Invoke(swapResults);


    }

    
    void UnregisterTileClicks()
    {
        firstTileClicked =                          null;
        secondTileClicked =                         null;
    }

    /// <summary>
    /// Checks if the two tiles can be swapped, and returns what kind of swap can be made between them.
    /// </summary>
    TileSwapType CanSwapTiles(TileController tile1, TileController tile2)
    {
        // First, we see if any of the swap rules are broken.

        // Rulebreak 1: Horizontal swaps involving air tiles
        bool horizSwap =                            tile1.BoardPos.x != tile2.BoardPos.x;
        if (horizSwap)
        {
            if (tile1.Type == airTileType || tile2.Type == airTileType)
                return TileSwapType.none;
        }

        // Rulebreak 2: Vertical swaps that involve an air tile and a non-air tile below it
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
                return TileSwapType.none;

        }

        // Now, we see if the two tiles make for an Adjacent Swap, or a Knight Swap. Adjacent Swaps have 
        // a one-tile distance on one axis, and a no-tile distance on the other. 
        // Knight Swaps have a two-tile distance on one axis, and a one-tile distance on the other. 
        // L-shape and whatnot.
        Vector2Int tileDist =                       new Vector2Int(Mathf.Abs(tile1.BoardPos.x - tile2.BoardPos.x), 
                                                    Mathf.Abs(tile1.BoardPos.y - tile2.BoardPos.y));

        bool adjacentSwap =                         (tileDist.x == 1 && tileDist.y == 0) || 
                                                    (tileDist.y == 1 && tileDist.x == 0);
        bool knightSwap =                           (tileDist.x == 2 && tileDist.y == 1) || 
                                                    (tileDist.y == 2 && tileDist.x == 1);
        bool airTileInvolved =                      firstTileClicked.type == airTileType || 
                                                    secondTileClicked.type == airTileType;
        if (adjacentSwap && airTileInvolved)
            return TileSwapType.freeAdjacent;
        else if (adjacentSwap)
            return TileSwapType.adjacent;
        else if (knightSwap)
            return TileSwapType.knight;
        else
            return TileSwapType.none;
    }

    void UpdateSwapResults(TileSwapType swapType)
    {
        swapResults.SwapType =                      swapType;
        List<TileController> tiles =                swapResults.TilesInvolved;
        tiles.Clear();
        tiles.Add(firstTileClicked);
        tiles.Add(secondTileClicked);
    }

    #endregion
    
}

public enum TileSwapType
{
    none, 
    adjacent, 
    knight,
    freeAdjacent,

}