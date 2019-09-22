using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

/// <summary>
/// Generic Fungus event
/// </summary>\
public abstract class SimpleEvent<TEventType> : EventHandler where TEventType: EventHandler
{
    public static void Invoke()
    {
        System.Type eventType = typeof(TEventType);
        foreach (TEventType eventObj in FindObjectsOfType(eventType))
            eventObj.ExecuteBlock();
    }
}

public abstract class SimpleEvent<TEventType, TEventArgsType>: SimpleEvent<TEventType> 
where TEventType: EventHandler
{
    public static void Invoke(TEventArgsType eventArgs)
    {
        System.Type eventType = typeof(TEventType);
        foreach (TEventType eventObj in FindObjectsOfType(eventType))
            eventObj.ExecuteBlock();
    }
}