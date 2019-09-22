using UnityEngine;
using Fungus;

[EventHandlerInfo("Three Knights",
                      "Tile Drag Entered",
                      "The block will execute when a tile is being dragged, and it starts touching another tile.")]
    [AddComponentMenu("")]
public class TileDragEnterEvent : EventHandler
{
    public static TileDragArgs lastTileDrag     { get; protected set; }
    public static void Invoke(TileDragArgs drag)
    {
        if (drag != null)
            lastTileDrag = drag;

        foreach (TileDragEnterEvent responder in FindObjectsOfType<TileDragEnterEvent>())
            responder.ExecuteBlock();
    }
}
