using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    public static void Flatten(this ref Vector3 v)
    {
        v.y = 0;
    }
}
