using System.Collections;
using UnityEngine;

public interface ISwapsTilePositions
{
    void ExecuteWith(TileSwapArgs context);
}

public abstract class TilePositionSwapper
{
    static MonoBehaviour allowsExecutingOverTime;
    delegate Coroutine OverTimeExecutor(IEnumerator toExecute);
    static OverTimeExecutor ExecuteOverTime;
    
    TileController firstTile, secondTile;
    float timePassedDuringSwap;
    Vector3 firstTileStartPos, secondTileStartPos;
    float moveDuration;
    float SwapProgress                              
    { 
        get 
        { 
            return Mathf.Clamp(timePassedDuringSwap / moveDuration, started, done); 
        } 
    }
    const int started =                             0;
    const int done =                                1;
    
    public abstract void ExecuteBasedOnContext(TileSwapArgs contextForSwap);

    public TilePositionSwapper(TileSwapArgs contextForSwap)
    {
        SetUpWhatAllowsExecutingOverTime();
        ExecuteOverTime =                           allowsExecutingOverTime.StartCoroutine;
        moveDuration =                              contextForSwap.MoveDuration;

        firstTile =                                 contextForSwap.TilesInvolved[0];
        secondTile =                                contextForSwap.TilesInvolved[1];
        secondTileStartPos =                        secondTile.transform.position;
        firstTileStartPos =                         firstTile.transform.position;
    }

    void SetUpWhatAllowsExecutingOverTime()
    {
        // For performance. We don't want a lot of GameObjects in the hierarchy, each only
        // existing to allow a single tile-swap to happen.
        if (SetupIsNecessary())
        {
            GameObject swapperGameObject =          new GameObject("TilePositionSwapper");
            allowsExecutingOverTime =               swapperGameObject.AddComponent<MonoBehaviour>();
        }
    }

    bool SetupIsNecessary()
    {
        return allowsExecutingOverTime == null;
    }

    public void Execute()
    {
        IEnumerator swapProcess =                   SwapPositions();
        ExecuteOverTime(swapProcess);
    }

    IEnumerator SwapPositions()
    {
        yield return ExecuteOverTime(SwapTilesPhysically());
        SwapAtTheGridLevel();
    }

    IEnumerator SwapTilesPhysically()
    {
        while (physicalSwapIsNotDone)
        {
            UpdateTimePassed();
            MoveTilesPhysically();
            yield return new WaitForFixedUpdate();
        }
    }

    bool physicalSwapIsNotDone                      { get { return SwapProgress != done; } }

    void UpdateTimePassed()
    {
        timePassedDuringSwap +=                     Time.deltaTime;
    }

    void MoveTilesPhysically()
    {
        Vector3 whereToMoveFirstTile =              Vector3.Lerp(firstTileStartPos, secondTileStartPos, 
                                                    SwapProgress);
        Vector3 whereToMoveSecondTile =             Vector3.Lerp(secondTileStartPos, firstTileStartPos, 
                                                    SwapProgress);
        firstTile.transform.position =              whereToMoveFirstTile;
        secondTile.transform.position =             whereToMoveSecondTile;
    }

    void SwapAtTheGridLevel()
    {
        Vector2Int firstBoardPos =                  firstTile.BoardPos;
        firstTile.BoardPos =                        secondTile.BoardPos;
        secondTile.BoardPos =                       firstBoardPos;
    }


}