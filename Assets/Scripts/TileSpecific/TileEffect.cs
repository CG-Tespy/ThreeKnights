using UnityEngine;

public abstract class TileEffect : GameEffect<TileEffArgs> 
{
    [TextArea(3, 6)]
    [SerializeField] string description;

    public override string Description
    {
        get { return description; }
    }
}
