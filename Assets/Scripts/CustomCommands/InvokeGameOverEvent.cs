using System;
using UnityEngine;
using Fungus;

[CommandInfo("Three Knights", 
                 "Invoke Game Over Event", 
                 "Runs blocks set to react to the Game Over event")]
    [AddComponentMenu("")]
public class InvokeGameOverEvent : Command
{
   
    public override void OnEnter()
    {
        base.OnEnter();
        GameOverEvent.Invoke();
    }
}
