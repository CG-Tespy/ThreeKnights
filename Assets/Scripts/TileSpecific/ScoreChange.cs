using UnityEngine;

[CreateAssetMenu(menuName = "Three Knights/Tile Effects/Score Change", fileName = "NewScoreChange")]
public class ScoreChange : TileEffect
{
    [SerializeField] NCOperator changeType =            NCOperator.add;
    [Tooltip("The number applied to another in this value change.")]
    [SerializeField] int number;

    public virtual int Number
    {
        get { return number; }
    }

    public override void Execute(TileEffArgs arg)
    {
        ApplyTo(arg.playerScore);
    }
    
    public virtual void ApplyTo(IScoreHandler toApplyTo)
    {
        switch (changeType)
        {
            case NCOperator.add:
                toApplyTo.score += number;
                return;
            case NCOperator.subtract:
                toApplyTo.score -=  number;
                return;
            case NCOperator.multiply:
                toApplyTo.score *=  number;
                return;
            case NCOperator.divide:
                toApplyTo.score /=  number;
                return;
            case NCOperator.modulo:
                toApplyTo.score %=  number;
                return;
            default:
                Debug.LogError("NCOperator " + changeType + " not accounted for.");
                return;
        }
    }

}

public enum NCOperator
{
    add, 
    subtract,
    multiply, 
    divide,
    modulo
}