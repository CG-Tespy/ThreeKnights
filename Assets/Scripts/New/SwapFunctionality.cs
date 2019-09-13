using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace ThreeKnights
{
    public abstract class SwapFunctionality
    {
        /// <summary>
        /// For any time this functionality finished executing.
        /// </summary>
        public static UnityAction<SwapFunctionality, TileSwapArgs> swapListeners = delegate {};
        public abstract Task Execute();
    }

    public class PhysicalSwap : SwapFunctionality
    {
        delegate Coroutine OverTimeExecutor(IEnumerator toExecute);

        protected TileController FirstTile                        
        { 
            get
            { 
                 return context.TilesInvolved[0]; 
            } 
        }
        protected TileController SecondTile                       
        { 
            get 
            { 
                return context.TilesInvolved[1]; 
            } 
        }
        protected float swapDuration                              
        { 
            get 
            { 
                return context.MoveDuration; 
            } 
        }
        protected TileSwapArgs context;
        
        protected Vector3 firstTileStartPos, secondTileStartPos;
        
        protected float timePassedDuringSwap;
        
        protected float SwapProgress                              
        { 
            get 
            { 
                return Mathf.Clamp(timePassedDuringSwap / swapDuration, started, done); 
            } 
        }
        protected const int started = 0;
        protected const int done = 1;
        
        /// <summary>
        /// The context gives the details needed to do its job.
        /// </summary>
        public static async Task ExecuteWithContext(TileSwapArgs context)
        {
            PhysicalSwap swap = new PhysicalSwap(context);
            await swap.Execute();
        }

        public PhysicalSwap(TileSwapArgs contextForSwap)
        {
            this.context = contextForSwap;
            EnsureThereAreEnoughTiles();
            RegisterTileStartPositions();
        }

        void EnsureThereAreEnoughTiles()
        {
            const int justEnoughForAPair = 2;

            if (context.TilesInvolved.Count != justEnoughForAPair)
            {
                string explainsTheProblem = @"Cannot have a physical swap without exactly 2 
                tiles involved.";
                throw new System.ArgumentException(explainsTheProblem);
            }
        }

        void RegisterTileStartPositions()
        {
            firstTileStartPos = FirstTile.transform.position;
            secondTileStartPos = SecondTile.transform.position;
        }

        public override async Task Execute()
        {
            await SwapTilesPhysically();
            LetTheWorldKnowThisFinished();
        }

        async Task SwapTilesPhysically()
        {
            while (notDoneYet)
            {
                UpdateTimePassed();
                MoveTiles();
                await WaitForTheNextFrame();
            }
        }

        bool notDoneYet                                 
        { 
            get 
            { 
                return SwapProgress != done;
            } 
        }

        void UpdateTimePassed()
        {
            timePassedDuringSwap += Time.deltaTime;
        }

        void MoveTiles()
        {
            Vector3 whereToMoveFirstTile = Vector3.Lerp(firstTileStartPos, secondTileStartPos, 
                                                        SwapProgress);
            Vector3 whereToMoveSecondTile = Vector3.Lerp(secondTileStartPos, firstTileStartPos, 
                                                        SwapProgress);
            FirstTile.transform.position = whereToMoveFirstTile;
            SecondTile.transform.position = whereToMoveSecondTile;
        }

        async Task WaitForTheNextFrame()
        {
            int millisecondsToWait = (int)(Time.fixedDeltaTime * 1000);
            await Task.Delay(millisecondsToWait);
        }

        void LetTheWorldKnowThisFinished()
        {
            swapListeners.Invoke(this, context);
        }

    }

    public class GridSwap : SwapFunctionality
    {
        TileController FirstTile                        
        { 
            get
            { 
                 return context.TilesInvolved[0]; 
            } 
        }
        TileController SecondTile                       
        { 
            get 
            { 
                return context.TilesInvolved[1]; 
            } 
        }
        TileSwapArgs context;
        
        public static async Task ExecuteWithContext(TileSwapArgs context)
        {
            GridSwap swap = new GridSwap(context);
            await swap.Execute();
        }

        public GridSwap(TileSwapArgs context)
        {
            this.context = context;
            EnsureThereAreEnoughTiles();
        }

        void EnsureThereAreEnoughTiles()
        {
            const int justEnoughForAPair = 2;

            if (context.TilesInvolved.Count != justEnoughForAPair)
            {
                string explainsTheProblem = @"Cannot have a grid swap without exactly 2 
                tiles involved.";
                throw new System.ArgumentException(explainsTheProblem);
            }
        }

        public override async Task Execute()
        {
            await Task.Run(() => SwapTheTilesGridPositions());
        }

        void SwapTheTilesGridPositions()
        {
            Vector2Int firstTilePos = FirstTile.BoardPos;
            FirstTile.BoardPos = SecondTile.BoardPos;
            SecondTile.BoardPos = firstTilePos;
        }
    }

    /// <summary>
    /// Knight Swaps are a series of Adjacent Swaps that make up an L shape.
    /// </summary>
    public class KnightSwap : PhysicalSwap
    {
        TileBoardController tileBoard;
        Vector2Int boardDistanceBetweenTiles
        {
            get
            {
                return SecondTile.BoardPos - FirstTile.BoardPos;
            }
        }

        new public static void ExecuteWithContext(TileSwapArgs context)
        {
            KnightSwap swap = new KnightSwap(context);
            swap.Execute();
        }

        public override Task Execute()
        {
            // Execute Horizontal Swaps
                // Decide what tiles will be involved in the horizontal swaps.
                    // if the horizontal distance is positive, get tiles to the right of the first
                    // otherwise if the horizontal distance is negative, get tiles to the left of the first
                // In order, swap the first tile with those tiles
                    // execute a physical swap between the first tile and current horizontal swap tile

            // Execute Vertical Swaps
                // Do similar stuff as the horizontal swap
            
            throw new System.NotImplementedException();
        }

        // TODO: Finish this function
        async Task ExecuteHorizontalSwaps()
        {
            IList<TileController> tilesToSwapTheFirstWith = GetHorizontalSwapTiles();
        
            foreach (TileController tileToSwapWith in tilesToSwapTheFirstWith)
            {
                TileSwapArgs swapContext = SetupSwapContextBetween(FirstTile, tileToSwapWith);
                await PhysicalSwap.ExecuteWithContext(swapContext);
            }

            throw new System.NotImplementedException();
        }

        public KnightSwap(TileSwapArgs context) : base(context)
        {
            tileBoard = FirstTile.Board;
        }

        IList<TileController> GetHorizontalSwapTiles()
        {
            int tilesToGet = Math.Abs(boardDistanceBetweenTiles.x);
            Vector2Int directionToCheck = new Vector2Int(boardDistanceBetweenTiles.x, 0);

            return tileBoard.GetTilesRelativeTo(FirstTile, tilesToGet, directionToCheck);
        }

        void ApplyHorizontalSwapBetween(TileController firstTile, TileController secondTile)
        {
            
        }

        TileSwapArgs SetupSwapContextBetween(TileController firstTile, TileController secondTile)
        {
            TileSwapArgs swapContext = new TileSwapArgs();
            swapContext.TilesInvolved.Add(firstTile);
            swapContext.TilesInvolved.Add(secondTile);
            return swapContext;
        }

        void ExecuteVerticalSwaps()
        {

        }

    }

}