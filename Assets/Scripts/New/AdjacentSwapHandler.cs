using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

public class AdjacentSwapHandler : SwapHandler
{
    #region Fields
    [Tooltip("Flowchart containing values shared by different systems.")]
    [SerializeField] Flowchart gameVals;
    [SerializeField] Flowchart tileSwapVals;
    [SerializeField] TileType airTileType;

    TileController firstTile, secondTile; // The tiles clicked

    TileSwapArgs swapContext =              new TileSwapArgs();

    #region Fungus Variables
    BooleanVariable swapEnabled;

    ObjectVariable airTileVar;
    
    StringVariable cancelAxisVar;

    FloatVariable swapDurationVar;
    BooleanVariable lastSwapFreeVar;
    #endregion
    #endregion

    #region Properties
    public bool SwapEnabled
    {
        get                                         { return swapEnabled.Value; }
        set                                         { swapEnabled.Value = value; }
    }

    TileType AirTileType                            { get { return airTileVar.Value as TileType; } }
    string CancelAxis                               { get { return cancelAxisVar.Value; } }
    float SwapDuration                              { get { return swapDurationVar.Value; } }

    bool LastSwapFree
    {
        get                                         { return lastSwapFreeVar.Value; }
        set                                         { lastSwapFreeVar.Value = value; }
    }
    #endregion

    protected virtual void Awake()
    {
        RegisterVariables();
        RegisterRules();
    }

    void Update()
    {
        // Let the player cancel their selection
        if (Input.GetAxis(CancelAxis) != 0 && SwapEnabled)
            UnregisterTileClicks();
    }

    void OnDestroy()
    {
        // Always make sure to clean up when necessary, if listening to static events!
        TileController.AnyClicked -=                OnAnyTileClicked;
    }

    protected virtual void OnAnyTileClicked(TileController tile)
    {
        if (!SwapEnabled)
            return;

        if (firstTile == null)
            firstTile =                         tile;
        else if (secondTile == null && firstTile != tile) // Make sure the tiles are different
            secondTile =                        tile;

        bool twoTilesClicked =                  firstTile != null && secondTile != null;

        if (twoTilesClicked)
        {
            UpdateSwapContext();
            if (NoRulesViolated())
            {
                SwapEnabled =                       false; // Let the swap finish before this can do another one
            }
        }
    }

    protected IEnumerator SwapTiles()
    {
        TileController tile1 =                  swapContext.TilesInvolved[0];
        TileController tile2 =                  swapContext.TilesInvolved[1];

        // Smoothly swap their physical positions
        Vector3 tile1Pos =                      tile1.transform.position;
        Vector3 tile2Pos =                      tile2.transform.position;

        float timer =                           0;
        while (timer < SwapDuration)
        {
            timer +=                            Time.deltaTime;

            tile1.transform.position =          Vector3.Lerp(tile1Pos, tile2Pos, timer / SwapDuration);
            tile2.transform.position =          Vector3.Lerp(tile2Pos, tile1Pos, timer / SwapDuration);

            yield return new WaitForFixedUpdate();
        }

        // Then do the same for their board positions
        Vector2Int firstBoardPos =              tile1.BoardPos;
        tile1.BoardPos =                        tile2.BoardPos;
        tile2.BoardPos =                        firstBoardPos;

        throw new System.NotImplementedException();
    }


    #region Helpers
    protected virtual void RegisterVariables()
    {
        swapEnabled =                                   gameVals.GetVariable("swapEnabled") as BooleanVariable;
        swapDurationVar =                           tileSwapVals.GetVariable("swapDuration") as FloatVariable;
        cancelAxisVar =                             tileSwapVals.GetVariable("cancelAxis") as StringVariable;
        airTileVar =                                gameVals.GetVariable("airTileType") as ObjectVariable;
        lastSwapFreeVar =                           tileSwapVals.GetVariable("lastSwapFree") as BooleanVariable;
    }

    protected virtual void RegisterRules()
    {
        swapRules.Add(HandlerAllowedToSwap);
        swapRules.Add(TwoTilesInvolved);
        swapRules.Add(NoAirTilesInvolved);
        swapRules.Add(AirTileOnTop);
    }

    protected virtual void UpdateSwapContext()
    {
        swapContext.TilesInvolved.Clear();

        if (firstTile != null)
            swapContext.TilesInvolved.Add(firstTile);
        if (secondTile != null)
            swapContext.TilesInvolved.Add(secondTile);

        swapContext.SwapType =          TileSwapType.adjacent;
    }

    protected virtual bool NoRulesViolated()
    {
        foreach (SwapRule applyRule in swapRules)
        {
            bool result =               applyRule(swapContext);
            bool ruleViolated =         result == false;

            if (ruleViolated)
                return false;
        }

        return true;
    }

    protected virtual void UnregisterTileClicks()
    {
        firstTile =                     null;
        secondTile =                    null;
    }
    #endregion

    #region Rules

    protected virtual bool HandlerAllowedToSwap(TileSwapArgs args)
    {
        return SwapEnabled;
    }

    protected virtual bool AirTileOnTop(TileSwapArgs args)
    {
        // Allows adjacent swaps involving air tiles, if said tiles are above
        // non-air tiles.
        
        // This rule depends on some others
        if (!TwoTilesInvolved(args))
            return false;

        if (NoAirTilesInvolved(args)) // This rule wouldn't even need to apply, so...
            return true;

        TileController tile1 =              args.TilesInvolved[0];
        TileController tile2 =              args.TilesInvolved[2];

        bool bothAirTiles =                 tile1.Type == airTileType && tile2.Type == airTileType;

        if (bothAirTiles) // No swapping two air tiles. That is pointless.
            return false;

        // Which is the air tile?
        TileController airTile =            null;
        TileController otherTile =          null;
        if (tile1.Type == airTileType)
        {
            airTile =                       tile1;
            otherTile =                     tile2;
        }
        else if (tile2.Type == airTileType)
        {
            airTile =                       tile2;
            otherTile =                     tile1;
        }

        return airTile.BoardPos.y > otherTile.BoardPos.y;

    }

    protected virtual bool TwoTilesInvolved(TileSwapArgs args)
    {
        return args.TilesInvolved.Count == 2;
    }

    protected virtual bool NoAirTilesInvolved(TileSwapArgs args)
    {
        foreach (TileController tile in args.TilesInvolved)
            if (tile.Type == airTileType)
                return false;

        return true;
    }

    #endregion

}
