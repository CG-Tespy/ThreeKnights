using UnityEngine;
using Fungus;

[EventHandlerInfo("Three Knights",
                      "Match Made",
                      "The block will execute when a tile move is made.")]
    [AddComponentMenu("")]
public class MoveMadeEvent : EventHandler
{
    public static TileMoveArgs lastMoveMade             { get; protected set; }
    public static void Invoke(TileMoveArgs moveMade)
    {
        if (moveMade != null)
            lastMoveMade =                              moveMade;
        foreach (MoveMadeEvent responder in FindObjectsOfType<MoveMadeEvent>())
        {
            responder.ExecuteBlock();
        }
    }
}
