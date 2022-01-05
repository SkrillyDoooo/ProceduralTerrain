using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TerrainPathFinder
{
}

public struct GridPoint
{
    public int X, Y;
    public GridPoint(int x, int y) { X = x; Y = y; }
    public GridPoint(float x, float y) { X = (int)x; Y = (int)y;  }

    public GridPoint(Vector3 v3) { X = (int)v3.x; Y = (int)v3.z;  }
    public GridPoint(Vector2 v2) { X = (int)v2.x; Y = (int)v2.y; }

}
