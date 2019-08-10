using UnityEngine;
using UnityEngine.Events;


public class TileController : MonoBehaviour
{
    [SerializeField] TileType type;
    public UnityAction Clicked =                            delegate {};
    public static UnityAction<TileController> AnyClicked =  delegate {};
    public static TileController lastClicked                { get; protected set; }

    public TileType Type
    {
        get { return type; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected virtual void OnMouseDown()
    {
        lastClicked =                       this;
        Clicked.Invoke();
        AnyClicked.Invoke(this);
    }
}
