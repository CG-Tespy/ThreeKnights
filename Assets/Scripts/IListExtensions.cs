using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IListExtensions 
{
     
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> toAddFrom)
    {
        foreach (T item in toAddFrom)
            list.Add(item);
    }
    
}
