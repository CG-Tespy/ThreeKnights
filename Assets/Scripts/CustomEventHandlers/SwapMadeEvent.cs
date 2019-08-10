using UnityEngine;
using Fungus;

[EventHandlerInfo("Three Knights",
                      "Match Made",
                      "The block will execute when a tile swap is made.")]
    [AddComponentMenu("")]
public class SwapMadeEvent : EventHandler
{
    public static TileSwapArgs lastSwapMade             { get; protected set; }
    public static void Invoke(TileSwapArgs swapMade)
    {
        if (swapMade != null)
            lastSwapMade =                              swapMade;
        foreach (SwapMadeEvent responder in FindObjectsOfType<SwapMadeEvent>())
        {
            responder.ExecuteBlock();
        }
    }
}
