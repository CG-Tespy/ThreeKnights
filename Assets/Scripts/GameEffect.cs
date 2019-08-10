using UnityEngine;

public abstract class GameEffect : ScriptableObject
{
    public virtual string Description               { get; set; }
}

public abstract class GameEffect<T> : GameEffect
{
    public abstract void Execute(T arg);
}

public abstract class GameEffect<T, TReturnVal> : GameEffect
{
    public abstract TReturnVal Execute(T arg);
}
