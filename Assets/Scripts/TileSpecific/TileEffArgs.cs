using System;
using System.Collections.Generic;

/// <summary>
/// Contains information a tile may want to know when executing its effects. Just in case we decide
/// to add fancy effects to tiles later on.
/// </summary>
public class TileEffArgs : EventArgs
{
    public List<TileAction> actionsInvolved =           new List<TileAction>();
    public List<TileType> tileTypesActive =             new List<TileType>();
    public IScoreHandler playerScore;

}

public enum TileAction
{
    match3, match4, match5, match6,
    move
}