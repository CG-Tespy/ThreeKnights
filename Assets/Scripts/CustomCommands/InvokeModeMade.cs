using UnityEngine;
using Fungus;

[CommandInfo("Three Knights", 
                 "Invoke Mode Made", 
                 "Triggers all the blocks that respond off of a move being made.")]
    [AddComponentMenu("")]
public class InvokeModeMade : Command
{
    public override void OnEnter()
    {
        base.OnEnter();
        MoveMadeEvent.Invoke(null);
        Continue();
    }
}
