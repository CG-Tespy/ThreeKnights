using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

[EventHandlerInfo("Three Knights",
                      "Tile Clicked",
                      "The block will execute when any tile is clicked.")]
    [AddComponentMenu("")]
public class TileClickEvent : EventHandler
{
    protected virtual void Awake()
    {
        TileController.AnyClicked += OnTileClicked;
    }

    protected virtual void OnTileClicked(TileController tileClicked)
    {
        ExecuteBlock();
    }

    protected virtual void OnDestroy()
    {
        // Best clean up as an event-listener should
        TileController.AnyClicked -= OnTileClicked;
    }
}
