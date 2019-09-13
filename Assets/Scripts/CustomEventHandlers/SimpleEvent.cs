using UnityEngine.Events;
using Fungus;

/// <summary>
/// Generic Fungus event
/// </summary>\
public abstract class SimpleEvent<TEventType> : EventHandler where TEventType: EventHandler
{
    // So code outside of a Fungus block can respond to any of these being triggered.
    public static void Invoke()
    {
        System.Type eventType =                 typeof(TEventType);
        foreach (TEventType eventObj in FindObjectsOfType(eventType))
            eventObj.ExecuteBlock();
    }
}

public abstract class SimpleEvent<TEventType, TEventArgsType>: SimpleEvent<TEventType> 
where TEventType: EventHandler
{
    public static void Invoke(TEventArgsType eventArgs)
    {
        System.Type eventType =                 typeof(TEventType);
        foreach (TEventType eventObj in FindObjectsOfType(eventType))
            eventObj.ExecuteBlock();
    }
}