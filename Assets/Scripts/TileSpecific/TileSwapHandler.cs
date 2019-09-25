using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Fungus;

using SysTask = System.Threading.Tasks.Task;

public class TileSwapHandler : MonoBehaviour
{
    #region Fields
    [Tooltip("Flowchart containing values multiple parts of the system use.")]
    [SerializeField] Flowchart gameVals;
    [Tooltip("Flowchart containing the primitive variable types this will need.")]
    [SerializeField] Flowchart tileSwapVals;
    [SerializeField] protected TileBoardController gameBoard;

    #region Fungus vars from Flowcharts
    ObjectVariable airTileVar;
    
    StringVariable cancelAxisVar;

    FloatVariable swapDurationVar;
    BooleanVariable swapEnabledVar;
    #endregion

    TileBoardController tileBoard;
    protected TileController firstTileClicked, secondTileClicked;
    TileSwapArgs swapResults = new TileSwapArgs();
    public static UnityAction<TileSwapHandler, TileSwapArgs> AnyPhysicalSwapMade = delegate {};
    public static UnityAction<TileSwapHandler, TileSwapArgs> AnyBoardSwapMade = delegate {};
    // ^ So custom code can respond to the swaps without having to be involved in a Fungus block.
    #endregion

    #region Properties
    protected TileType AirTileType                  { get { return airTileVar.Value as TileType; } }
    string CancelAxis                               { get { return cancelAxisVar.Value; } }
    float SwapDuration                              { get { return swapDurationVar.Value; } }
    protected bool SwapEnabled                                
    { 
        get                                         { return swapEnabledVar.Value; } 
        set                                         { swapEnabledVar.Value = value; }
    }
    #endregion

    protected virtual void Awake()
    {
        tileBoard = FindObjectOfType<TileBoardController>();
        TileController.AnyClicked += OnAnyTileClicked;
        swapDurationVar = tileSwapVals.GetVariable("swapDuration") as FloatVariable;
        swapEnabledVar = tileSwapVals.GetVariable("swapEnabled") as BooleanVariable;
        cancelAxisVar = tileSwapVals.GetVariable("cancelAxis") as StringVariable;
        airTileVar = gameVals.GetVariable("airTileType") as ObjectVariable;
        AnyPhysicalSwapMade += this.OnAnyPhysicalSwapMade;
    }

    protected virtual void OnDestroy()
    {
        // Always make sure to clean up when necessary, if listening to static events!
        TileController.AnyClicked -= OnAnyTileClicked;
        AnyPhysicalSwapMade -= this.OnAnyPhysicalSwapMade;
    }

    protected virtual void OnAnyPhysicalSwapMade(TileSwapHandler handler, TileSwapArgs swapArgs)
    {
        this.Reset();
    }

    void Update()
    {
        // Let the player cancel their selection
        if (Input.GetAxis(CancelAxis) != 0 && SwapEnabled)
            UnregisterTiles();
    }

    protected virtual void OnAnyTileClicked(TileController tile)
    {
        if (!SwapEnabled || ShouldIgnoreTile(tile))
            return;
        
        RegisterTile(tile);

        if (ShouldReset())
        {
            this.Reset();
            return;
        }

        if (TwoTilesClicked()) // Consider swapping the tiles.
        {
            ConsiderSwappingTiles();
        }

    }

    protected void RegisterTile(TileController tile)
    {
        if (firstTileClicked == null)
            firstTileClicked = tile;
        else if (secondTileClicked == null && firstTileClicked != tile)
            secondTileClicked = tile;
    }

    protected virtual bool ShouldIgnoreTile(TileController tile)
    {
        return TileNotOnGameBoard(tile);
    }

    bool TileNotOnGameBoard(TileController tile)
    {
        return tile.Board != this.gameBoard;
    }

    protected virtual bool ShouldReset()
    {
        return FirstTileClickedIsAir();
    }

    bool FirstTileClickedIsAir()
    {
        return firstTileClicked != null && firstTileClicked.Type == AirTileType;
    }
    bool OnlyOneTileClicked()
    {
        return firstTileClicked != null && secondTileClicked == null;
    }

    bool IsAirTile(TileController tile)
    {
        return tile.Type == AirTileType;
    }

    protected bool TwoTilesClicked()
    {
        return firstTileClicked != null && secondTileClicked != null;
    }

    protected virtual void ConsiderSwappingTiles()
    {
        TileSwapType swapType = CanSwapTiles(firstTileClicked, secondTileClicked);

        if (swapType != TileSwapType.none) // We have a valid swap attempt here!
        {
            SwapEnabled = false; // Will be reenabled once the swap is done
            SwapTask(firstTileClicked, secondTileClicked, swapType);
            //StartCoroutine(SwapCoroutine(firstTileClicked, secondTileClicked, swapType));
        }
        else
            UnregisterTiles();
    }

    #region Helpers

    protected virtual async SysTask SwapTask(TileController firstTile, TileController secondTile, TileSwapType swapType)
    {
        switch (swapType)
        {
            case TileSwapType.adjacent:
                await AdjacentSwapTask(firstTile, secondTile);
                break;
            case TileSwapType.freeAdjacent:
                await AdjacentSwapTask(firstTile, secondTile);
                break;
            case TileSwapType.gravityControl:
                await AdjacentSwapTask(firstTile, secondTile);
                break;
            case TileSwapType.knight:
                await KnightSwapTask(firstTile, secondTile);
                break;
            default:
                Debug.LogError("TileSwapType " + swapType + " not accounted for!");
                break;
        }

        UpdateSwapResults(swapType);
        UnregisterTiles();
        AlertSwapListeners(swapType);
        SwapEnabled = true;
    }

    protected virtual async SysTask AdjacentSwapTask(TileController firstTile, TileController secondTile)
    {
        // Has the tiles move as they're swapped. Helps see if the swapping is even happening correctly.
        Vector3 firstPos = firstTile.transform.position;
        Vector3 secondPos = secondTile.transform.position;

        float swapTimer = 0f;
        int untilNextFixedUpdate = (int)(SwapDuration * Time.fixedDeltaTime * 1000);

        while (swapTimer < SwapDuration)
        {
            swapTimer += Time.deltaTime;
            firstTile.transform.position = Vector3.Lerp(firstPos, secondPos, 
                                                    swapTimer / SwapDuration);
            secondTile.transform.position = Vector3.Lerp(secondPos, firstPos, 
                                                    swapTimer / SwapDuration);

            untilNextFixedUpdate = (int)(SwapDuration * Time.fixedDeltaTime * 1000);
            await SysTask.Delay(untilNextFixedUpdate);
        }

        SwapBoardPositions(firstTile, secondTile);

    }

    protected virtual async SysTask KnightSwapTask(TileController firstTile, TileController secondTile)
    {
        Vector2Int travelVec = secondTile.BoardPos - firstTile.BoardPos;

        // Pull off all the horizontal swaps before the vertical ones
        while (travelVec.x != 0)
        {
            // Find the appropriate adjacent tile, swap the first one with it, and update 
            // the travelVec accordingly.
            int xOffset = (int) Mathf.Sign(travelVec.x);
            Vector2Int otherTilePos = firstTile.BoardPos;
            otherTilePos.x += xOffset;
            TileController toSwapWith = tileBoard.GetTileAt(otherTilePos);

            await AdjacentSwapTask(firstTile, toSwapWith);
            travelVec.x -= xOffset; // Bring the value closer to 0
        }

        // Similar logic for the vertical swaps
        while (travelVec.y != 0)
        {
            int xOffset = (int) Mathf.Sign(travelVec.y);
            Vector2Int otherTilePos = firstTile.BoardPos;
            otherTilePos.y += xOffset;
            TileController toSwapWith = tileBoard.GetTileAt(otherTilePos);

            await AdjacentSwapTask(firstTile, toSwapWith);
            travelVec.y -= xOffset;
        }

    }

    protected virtual void SwapBoardPositions(TileController firstTile, TileController secondTile)
    {
        Vector2Int firstBoardPos = firstTile.BoardPos;
        firstTile.BoardPos = secondTile.BoardPos;
        secondTile.BoardPos = firstBoardPos;

        UpdateSwapResults(TileSwapType.freeAdjacent, firstTile, secondTile);
        AnyBoardSwapMade.Invoke(this, swapResults);
    }
    void Reset()
    {
        UnregisterTiles();
    }

    void UnregisterTiles()
    {
        firstTileClicked = null;
        secondTileClicked = null;
    }

    /// <summary>
    /// Checks if the two tiles can be swapped, and returns what kind of swap can be made between them.
    /// </summary>
    protected virtual TileSwapType CanSwapTiles(TileController tile1, TileController tile2)
    {
        // First, we see if any of the swap rules are broken.

        // Rulebreak 1: The first tile being an air tile
        bool firstTileIsAir = tile1.Type == AirTileType;
        if (firstTileIsAir)
            return TileSwapType.none;

        // Rulebreak 2: Horizontal swaps involving air tiles
        bool horizSwap = tile1.BoardPos.x != tile2.BoardPos.x;
        if (horizSwap)
        {
            if (tile1.Type == AirTileType || tile2.Type == AirTileType)
                return TileSwapType.none;
        }

        // Rulebreak 3: Vertical swaps that involve an air tile and a non-air tile below it
        bool vertSwap = tile1.BoardPos.y != tile2.BoardPos.y;
        if (vertSwap)
        {
            TileController higherTile = null;
            TileController lowerTile = null;

            if (tile1.BoardPos.y > tile2.BoardPos.y)
            {
                higherTile = tile1;
                lowerTile = tile2;
            }
            else
            {
                higherTile = tile2;
                lowerTile = tile1;
            }

            if (higherTile.Type == AirTileType)
                return TileSwapType.none;

        }

        // Now, we see if the two tiles make for an Adjacent Swap, or a Knight Swap. Adjacent Swaps have 
        // a one-tile distance on one axis, and a no-tile distance on the other. 
        // Knight Swaps have a two-tile distance on one axis, and a one-tile distance on the other. 
        // L-shape and whatnot.
        Vector2Int tileDist = new Vector2Int(Mathf.Abs(tile1.BoardPos.x - tile2.BoardPos.x), 
                                                    Mathf.Abs(tile1.BoardPos.y - tile2.BoardPos.y));

        bool adjacentSwap = (tileDist.x == 1 && tileDist.y == 0) || 
                                                    (tileDist.y == 1 && tileDist.x == 0);
        bool knightSwap = (tileDist.x == 2 && tileDist.y == 1) || 
                                                    (tileDist.y == 2 && tileDist.x == 1);
        bool airTileInvolved = firstTileClicked.Type == AirTileType || 
                                                    secondTileClicked.Type == AirTileType;
        if (adjacentSwap && airTileInvolved)
            return TileSwapType.gravityControl;
        else if (adjacentSwap)
            return TileSwapType.adjacent;
        else if (knightSwap)
            return TileSwapType.knight;
        else
            return TileSwapType.none;
    }

    void UpdateSwapResults(TileSwapType swapType, TileController firstTile = null, TileController secondTile = null)
    {
        swapResults.SwapType = swapType;
        List<TileController> tiles = swapResults.TilesInvolved;
        tiles.Clear();

        if (firstTile == null)
            tiles.Add(firstTileClicked);
        else
            tiles.Add(firstTile);

        if (secondTile == null)
            tiles.Add(secondTileClicked);
        else 
            tiles.Add(secondTile);
    }

    void AlertSwapListeners(TileSwapType swapType)
    {
        // Whether it's Fungus blocks or custom code.
        AnyPhysicalSwapMade.Invoke(this, swapResults);

        if (swapType != TileSwapType.freeAdjacent && 
            swapType != TileSwapType.gravityControl) // Don't let the Fungus system count this as a swap
            SwapMadeEvent.Invoke(swapResults);
    }

    void AlertNonFungusSwapListeners()
    {
        AnyPhysicalSwapMade.Invoke(this, swapResults);
    }

    #endregion
    
}

public enum TileSwapType
{
    none, 
    adjacent, 
    knight,
    freeAdjacent,
    gravityControl
}