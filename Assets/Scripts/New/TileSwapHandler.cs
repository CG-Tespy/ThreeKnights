using UnityEngine;
using System.Collections.Generic;

namespace ThreeKnights
{
    public abstract class TileSwapHandler : MonoBehaviour
    {
        protected List<SwapRule> swapRules =        new List<SwapRule>();
        public virtual List<SwapRule> SwapRules
        {
            get                                     { return swapRules; }
        }
        
    }

    /// <summary>
    /// Delegate for functions that decide if a potential swap is to be allowed.
    /// </summary>
    public delegate bool SwapRule(TileSwapArgs args);
}

