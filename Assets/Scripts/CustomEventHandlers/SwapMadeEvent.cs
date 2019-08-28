using UnityEngine;
using Fungus;

[EventHandlerInfo("Three Knights",
                      "Swap Made",
                      "The block will execute when a tile swap is made.")]
    [AddComponentMenu("")]
public class SwapMadeEvent : SimpleEvent<SwapMadeEvent, TileSwapArgs> {}
