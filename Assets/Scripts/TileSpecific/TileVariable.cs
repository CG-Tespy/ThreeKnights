using UnityEngine;
using Fungus;

[VariableInfo("", "Tile")]
[AddComponentMenu("")]
[System.Serializable]
public class TileVariable : VariableBase<TileController>
{
    public static readonly CompareOperator[] compareOperators = { CompareOperator.Equals, CompareOperator.NotEquals };
    public static readonly SetOperator[] setOperators = { SetOperator.Assign };

    public virtual bool Evaluate(CompareOperator compareOperator, TileController value)
    {
        bool condition = false;

        switch (compareOperator)
        {
            case CompareOperator.Equals:
                condition = Value == value;
                break;
            case CompareOperator.NotEquals:
                condition = Value != value;
                break;
            default:
                Debug.LogError("The " + compareOperator.ToString() + " comparison operator is not valid.");
                break;
        }

        return condition;
    }

    public override void Apply(SetOperator setOperator, TileController value)
    {
        switch (setOperator)
        {
            case SetOperator.Assign:
                Value = value;
                break;
            default:
                Debug.LogError("The " + setOperator.ToString() + " set operator is not valid.");
                break;
        }
    }

}

/// <summary>
/// Container for an TileController variable reference or constant value.
/// </summary>
[System.Serializable]
public struct TileData
{
    [SerializeField]
    [VariableProperty("<Value>", typeof(TileVariable))]
    public TileVariable TileControllerRef;
    
    [SerializeField]
    public TileController TileControllerVal;

    public static implicit operator TileController(TileData TileData)
    {
        return TileData.Value;
    }

    public TileData(TileController v)
    {
        TileControllerVal = v;
        TileControllerRef = null;
    }

    public TileController Value
    {
        get { return (TileControllerRef == null) ? TileControllerVal : TileControllerRef.Value; }
        set { if (TileControllerRef == null) { TileControllerVal = value; } else { TileControllerRef.Value = value; } }
    }

    public string GetDescription()
    {
        if (TileControllerRef == null)
        {
            return TileControllerVal.ToString();
        }
        else
        {
            return TileControllerRef.Key;
        }
    }
}