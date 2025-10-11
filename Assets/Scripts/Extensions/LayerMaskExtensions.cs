using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerMaskExtensions 
{
    public static bool ContainsLayer(this LayerMask mask, int layer)
    {
        return mask == (mask | 1 << layer);
    }
}
