using System;
using System.Collections.Generic;
using UnityEngine;

// When a tile on one board is first selected, all the tiles in another are highlighted.
public class InterboardTileHighlighter : SelectableTileHighlighter
{
    [SerializeField] TileBoardController[] otherBoards;
    IList<TileBoardController> canHighlightOn = new List<TileBoardController>();

    protected override void Awake()
    {
        base.Awake();
        EstablishWhatThisCanHighlightOn();
    }

    void EstablishWhatThisCanHighlightOn()
    {
        canHighlightOn.Add(gameBoard);
        canHighlightOn.AddRange(otherBoards);
    }

    protected override bool ShouldResetThis()
    {
        return TileIsSecondClicked();
    }

    protected override bool ShouldPlaceHighlighters()
    {
        return !ShouldResetThis() && !IsAirTile(tileClicked) && !ClickedCenterTile();
    }

    bool TilesAreOnDifferentBoards(TileController firstTile, TileController secondTile)
    {
        return firstTile.Board != secondTile.Board;
    }

    protected override void HighlightAppropriateTiles()
    {
        HighlightTilesInOtherBoards();
    }

    void HighlightTilesInOtherBoards()
    {
        foreach (TileBoardController board in canHighlightOn)
        {
            if (IsOtherBoard(board))
            {
                HighlightTilesInBoard(board);
            }
        }
    }

    bool IsOtherBoard(TileBoardController board)
    {
        return board != centerTile.Board;
    }

    void HighlightTilesInBoard(TileBoardController board)
    {
        for (int x = 0; x < board.ColumnCount; x++)
        {
            for (int y = 0; y < board.RowCount; y++)
            {
                TileController tile = board.Tiles[x, y];
                Transform newHighlighter = SetupHighlighter();
                PlaceHighlighterOnTile(newHighlighter, tile);
                RegisterTileAsHighlighted(tile);
            }
        }
    }

    protected override void Reset()
    {
        base.Reset();
    }
}
