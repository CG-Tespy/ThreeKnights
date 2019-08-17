using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies the Gravity Control mechanic, where when a move is made, the tiles involved
/// become air tiles that can be freely swapped with vertically-adjacent ones. Tiles that are swapped
/// with air tiles do not become air tiles themselves.
/// </summary>
public class GravityControlHandler : MonoBehaviour
{
    [SerializeField] TileType airTileType;
    [SerializeField] TileController airTilePrefab;

    void Awake()
    {
        airTilePrefab.Type =                    airTileType;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnSwapMade(TileSwapHandler swapHandler, TileSwapArgs swapArgs)
    {
        if (!ContainsAirTile(swapArgs.TilesInvolved))
        {
            // We turn the tiles involved into air tiles.
        }


    }

    bool ContainsAirTile(ICollection<TileController> tiles)
    {
        foreach (TileController tile in tiles)
            if (tile.Type == airTileType)
                return true;
        
        return false;
    }
}
