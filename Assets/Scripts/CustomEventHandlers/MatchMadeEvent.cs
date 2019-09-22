using UnityEngine;
using Fungus;

[EventHandlerInfo("Three Knights",
                      "Match Made",
                      "The block will execute when a tile match is made.")]
    [AddComponentMenu("")]
public class MatchMadeEvent : SimpleEvent<MatchMadeEvent>
{
    public static MatchArgs latestMatch                 { get; protected set; }
    public static void Invoke(MatchArgs matchMade)
    {
        if (matchMade != null)
            latestMatch = matchMade;

        foreach (MatchMadeEvent matchEvent in FindObjectsOfType<MatchMadeEvent>())
        {
            matchEvent.ExecuteBlock();
        }
    }
}
