using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Fungus;

public class SelectableTileHighlighter : MonoBehaviour
{
    [SerializeField] Flowchart gameVals;
    [SerializeField] protected Transform highlighter;
    [SerializeField] protected Transform highlighterGroup;
    [SerializeField] protected TileBoardController gameBoard;
    protected TileController centerTile;
    protected TileController tileClicked;

    ObjectVariable airTileVar;
    TileType AirTileType
    {
        get { return airTileVar.Value as TileType; }
    }
    
    protected IList<Transform> activeHighlighters = new List<Transform>();
    protected IList<TileController> highlightedTiles = new List<TileController>();
    Vector2Int CenterTilePosition 
    {
        get
        {
            return centerTile.BoardPos;
        }
    }
    
    protected virtual void Awake()
    {
        SetToHighlightWhenTileClicked();
        SetToResetWhenSwapHappens();
        RegisterTheAirTileType();
    }

    void SetToHighlightWhenTileClicked()
    {
        TileController.AnyClicked += OnAnyTileClicked;
    }

    void SetToResetWhenSwapHappens()
    {
        TileSwapHandler.AnySwapMade += OnAnySwapMade;
    }

    void RegisterTheAirTileType()
    {
        airTileVar = gameVals.GetVariable("airTileType") as ObjectVariable;
    }

    protected virtual void OnDestroy()
    {
        StopListeningForEvents();
    }

    void StopListeningForEvents()
    {
        TileController.AnyClicked -= OnAnyTileClicked;
        TileSwapHandler.AnySwapMade -= OnAnySwapMade;
    }

    protected virtual void OnAnySwapMade(TileSwapHandler swapHandler, TileSwapArgs args)
    {
        this.Reset();
    }

    protected virtual void OnAnyTileClicked(TileController tile)
    {
        if (this.NotAllowedToAct())
            return;

        RegisterTileClicked(tile);

        if (ShouldResetThis())
        {
            this.Reset();
        }
        else if (ShouldPlaceHighlighters())
        {
            RegisterAsCenterTile(tile);
            HighlightAppropriateTiles();
        }
    }

    protected virtual bool NotAllowedToAct()
    {
        return this.enabled == false || this.gameObject.activeSelf == false;
    }

    void RegisterTileClicked(TileController tile)
    {
        tileClicked = tile;
    }

    protected virtual bool ShouldResetThis()
    {
        return TileIsSecondClicked() || !TileOnGameBoard(tileClicked) || IsAirTile(tileClicked);
    }

    protected bool TileIsSecondClicked()
    {
        return centerTile != null && centerTile != tileClicked;
    }

    protected virtual bool TileOnGameBoard(TileController tile)
    {
        return tile.Board == this.gameBoard;
    }

    protected bool IsAirTile(TileController tile)
    {
        return tile.Type == AirTileType;
    }

    protected virtual bool ShouldPlaceHighlighters()
    {
        return !ShouldResetThis() && !ClickedCenterTile() && !IsAirTile(tileClicked);
    }

    protected bool OnlyOneTileClicked()
    {
        throw new System.NotImplementedException(); 
    }

    protected bool ClickedCenterTile()
    {
        return tileClicked == centerTile;
    }

    protected bool ThisIsNotHighlightingTiles()
    {
        return this.centerTile == null;
    }

    protected virtual void HighlightAppropriateTiles()
    {
        HighlightTilesAroundCenter();
    }

    protected virtual void HighlightTilesAroundCenter()
    {
        IList<TileController> tilesToHighlight = GetTilesToHighlight();

        foreach (TileController tile in tilesToHighlight)
        {
            Transform newHighlighter = SetupHighlighter();
            PlaceHighlighterOnTile(newHighlighter, tile);
            RegisterTileAsHighlighted(tile);
        }
    }

    protected virtual IList<TileController> GetTilesToHighlight()
    {
        IList<Vector2Int> tilePositions = GetPositionsForTilesToHighlight();
        IList<TileController> tilesToHighlight = GetValidTilesAtPositions(tilePositions);

        return tilesToHighlight;
    }

    IList<Vector2Int> GetPositionsForTilesToHighlight()
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        positions.AddRange(GetPositionsAdjacentToCenterTile());
        positions.AddRange(GetPositionsToTheLeftOfCenterTile());
        positions.AddRange(GetPositionsToTheRightOfCenterTile());

        return positions;
    }

    IList<Vector2Int> GetPositionsAdjacentToCenterTile()
    {
        IList<Vector2Int> positions = new List<Vector2Int>();

        positions.Add(CenterTilePosition + Vector2Int.right);
        positions.Add(CenterTilePosition + Vector2Int.left);
        positions.Add(CenterTilePosition + Vector2Int.up);
        positions.Add(CenterTilePosition + Vector2Int.down);

        return positions;
    }

    IList<Vector2Int> GetPositionsToTheLeftOfCenterTile()
    {
        IList<Vector2Int> positions = new List<Vector2Int>();

        positions.Add(CenterTilePosition + new Vector2Int(-2, 1));
        positions.Add(CenterTilePosition + new Vector2Int(-2, -1));
        positions.Add(CenterTilePosition + new Vector2Int(-1, 2));
        positions.Add(CenterTilePosition + new Vector2Int(-1, -2));

        return positions;
    }

    IList<Vector2Int> GetPositionsToTheRightOfCenterTile()
    {
        IList<Vector2Int> positions = new List<Vector2Int>();

        positions.Add(CenterTilePosition + new Vector2Int(2, 1));
        positions.Add(CenterTilePosition + new Vector2Int(2, -1));
        positions.Add(CenterTilePosition + new Vector2Int(1, 2));
        positions.Add(CenterTilePosition + new Vector2Int(1, -2));

        return positions;
    }

    IList<TileController> GetValidTilesAtPositions(IList<Vector2Int> tilePositions)
    {
        IList<TileController> validTiles = new List<TileController>();

        foreach (Vector2Int position in tilePositions)
        {
            TileController tile = gameBoard.GetTileAt(position);
            if (TileIsValid(tile))
                validTiles.Add(tile);
        }

        return validTiles;
    }

    bool TileIsValid(TileController tile)
    {
        return tile != null;
    }

    protected virtual Transform SetupHighlighter()
    {
        Transform newHighlighter = CreateHighlighter();
        RegisterHighlighter(newHighlighter);
        PutHighlighterInGroup(newHighlighter);
        return newHighlighter;
    }
    Transform CreateHighlighter()
    {
        return Instantiate<Transform>(highlighter);
    }

    void RegisterHighlighter(Transform highlighter)
    {
        activeHighlighters.Add(highlighter);
    }

    void PutHighlighterInGroup(Transform highlighter)
    {
        highlighter.SetParent(highlighterGroup, false);
    }

    protected virtual void PlaceHighlighterOnTile(Transform highlighter, TileController tile)
    {
        Vector3 inFrontOfTile = tile.transform.position + Vector3.back;
        highlighter.position = inFrontOfTile;
    }

    protected virtual void RegisterTileAsHighlighted(TileController tile)
    {
        highlightedTiles.Add(tile);
    }

    void EraseHighlighters()
    {
        for (int i = 0; i < activeHighlighters.Count; i++)
        {
            Transform highlighter = activeHighlighters[i];
            Destroy(highlighter.gameObject);
        }
    }

    void UnregisterHighlighters()
    {
        activeHighlighters.Clear();
    }

    void RegisterAsCenterTile(TileController tile)
    {
        this.centerTile = tile;
    }

    void UnregisterCenterTile()
    {
        this.centerTile = null;
    }

    void UnregisterHighlightedTiles()
    {
        highlightedTiles.Clear();
    }

    void Update() // Executes once per frame
    {
        ResetOnRightClick();
    }

    void ResetOnRightClick()
    {
        const int rightMouseButton = 1;
        if (Input.GetMouseButtonDown(rightMouseButton))
            this.Reset();
    }

    protected virtual void Reset()
    {
        EraseHighlighters();
        UnregisterHighlighters();
        UnregisterCenterTile();
        UnregisterHighlightedTiles();
    }
}
