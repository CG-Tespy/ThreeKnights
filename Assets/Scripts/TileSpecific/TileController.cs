using UnityEngine;
using UnityEngine.Events;


public class TileController : MonoBehaviour
{
    [SerializeField] TileType type;
    [SerializeField] Vector2Int boardPos;
    public UnityAction Clicked = delegate {};
    new protected Renderer renderer;

    // for debugging
    [SerializeField] TileBoardController board;

    /// <summary>
    /// Args: tile, oldType, newType
    /// </summary>
    public static UnityAction<TileController, TileType, TileType> AnyTypeChanged = delegate {};
    public static UnityAction<TileController> AnyClicked = delegate {};
    public static TileController lastClicked                { get; protected set; }
    public Vector2Int BoardPos 
    { 
        get                                                 { return boardPos; } 
        set                                                 { boardPos.Set(value.x, value.y); }
    }

    public TileType Type
    {
        get                                                 { return type; }
        set                                                 
        {
            TileType oldType = type;
            type = value;
            renderer.material = value.Material;
            AnyTypeChanged.Invoke(this, oldType, type);
        }
    }

    public TileBoardController Board 
    { 
        get { return this.board; }
        set { this.board = value; } 
    }

    void Awake()
    {
        renderer = GetComponent<Renderer>();
        if (type != null && type.Material != null)
            renderer.material = type.Material;
    }

    protected virtual void OnMouseDown()
    {
        lastClicked = this;
        Clicked.Invoke();
        AnyClicked.Invoke(this);
    }

    
}
