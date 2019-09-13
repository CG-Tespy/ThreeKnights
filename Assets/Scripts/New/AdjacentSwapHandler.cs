using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

namespace ThreeKnights
{
    public class AdjacentSwapHandler : ThreeKnights.TileSwapHandler
    {
        #region Fields
        [Tooltip("Flowchart containing values shared by different systems.")]
        [SerializeField] Flowchart gameVals;
        [SerializeField] Flowchart tileSwapVals;
        [SerializeField] TileType airTileType;

        TileController firstTileClicked, secondTile; // The tiles clicked

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
            InstateRules();
            swapContext.MoveDuration =                  SwapDuration;
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

            if (firstTileClicked == null)
                firstTileClicked =                         tile;
            else if (secondTile == null && firstTileClicked != tile) // Make sure the tiles are different
                secondTile =                        tile;

            bool twoTilesClicked =                  firstTileClicked != null && secondTile != null;

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
            TileController firstTile =                  swapContext.TilesInvolved[0];
            TileController secondTile =                 swapContext.TilesInvolved[1];

            // Smoothly swap their physical positions
            Vector3 firstTilePos =                      firstTile.transform.position;
            Vector3 secondTilePos =                     secondTile.transform.position;

            float timer =                               0;
            while (timer < SwapDuration)
            {
                timer +=                                Time.deltaTime;

                firstTile.transform.position =          Vector3.Lerp(firstTilePos, secondTilePos, timer / SwapDuration);
                secondTile.transform.position =         Vector3.Lerp(secondTilePos, firstTilePos, timer / SwapDuration);

                yield return new WaitForFixedUpdate();
            }

            // Then do the same for their board positions
            Vector2Int firstBoardPos =                  firstTile.BoardPos;
            firstTile.BoardPos =                        secondTile.BoardPos;
            secondTile.BoardPos =                       firstBoardPos;

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

        protected virtual void InstateRules()
        {
            swapRules.Add(HandlerAllowedToSwap);
            swapRules.Add(TwoTilesInvolved);
            swapRules.Add(NoAirTilesInvolved);
            swapRules.Add(AirTileOnTop);
        }

        protected virtual void UpdateSwapContext()
        {
            swapContext.TilesInvolved.Clear();

            if (firstTileClicked != null)
                swapContext.TilesInvolved.Add(firstTileClicked);
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
            firstTileClicked =                     null;
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

            TileController firstTile =              args.TilesInvolved[0];
            TileController secondTile =              args.TilesInvolved[2];

            bool bothAirTiles =                 firstTile.Type == airTileType && secondTile.Type == airTileType;

            if (bothAirTiles) // No swapping two air tiles. That is pointless.
                return false;

            // Which is the air tile?
            TileController airTile =            null;
            TileController otherTile =          null;
            if (firstTile.Type == airTileType)
            {
                airTile =                       firstTile;
                otherTile =                     secondTile;
            }
            else if (secondTile.Type == airTileType)
            {
                airTile =                       secondTile;
                otherTile =                     firstTile;
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
}