using UnityEngine;
using Fungus;

[EventHandlerInfo("Three Knights",
                      "Game Over",
                      "The block will execute when it's Game Over.")]
    [AddComponentMenu("")]
public class GameOverEvent : EventHandler
{
    public static void Invoke()
    {
        foreach (GameOverEvent gameOverEvent in FindObjectsOfType<GameOverEvent>())
        {
            gameOverEvent.ExecuteBlock();
        }
    }
}
