using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TerrainPathFinder
{
    TerrainPathFinderSettings settings;
    float GridStepSize = 2.5f;
    float WithinRangeRadius = 5;

    Dictionary<GridPoint, bool> closedSet = new Dictionary<GridPoint, bool>();
    Dictionary<GridPoint, bool> openSet = new Dictionary<GridPoint, bool>();

    //cost of start to this key node
    Dictionary<GridPoint, float> gScore = new Dictionary<GridPoint, float>();
    //cost of start to goal, passing through key node
    Dictionary<GridPoint, float> fScore = new Dictionary<GridPoint, float>();

    Dictionary<GridPoint, GridPoint> nodeLinks = new Dictionary<GridPoint, GridPoint>();
    AbstractPathfindingNode m_CurrentAbstractNode;

    int tryCountMax = 1000;
    int tryCount;
    GridPoint currentStart;
    float chunkWorldSize;

    const float D = 1;
    const float D2 = 1.41421356237f;

    public TerrainPathFinder(TerrainPathFinderSettings settings)
    {
        this.settings = settings;
    }

    public IEnumerator FindPath(TerrainGenerator terrain, AbstractPathfinder abstractPathFinder, GridPoint start, GridPoint goal, int refinementLevel, Stack<GridPoint> gridPoints, Action<bool> PathingCompleteCallback, Func<GridPoint, bool> debugCallback = null, Action<GridPoint, bool> openSetCallback = null, Action<GridPoint, bool> closedSetCallback = null)
    {
        refinementLevel = (refinementLevel == 0) ? 1 : refinementLevel;
        GridStepSize = TerrainGenerator.Instance.meshSettings.meshScale * refinementLevel;
        WithinRangeRadius = TerrainGenerator.Instance.meshSettings.meshScale * refinementLevel;
        chunkWorldSize = TerrainGenerator.Instance.meshSettings.meshWorldSize / (refinementLevel == 16 ? 1 : 2);

        closedSet.Clear();
        openSet.Clear();
        gScore.Clear();
        fScore.Clear();
        nodeLinks.Clear();

        openSet[start] = true;
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);
        tryCount = 0;
        currentStart = start;

        while (openSet.Count > 0)
        {
            var current = nextBest();
            if (debugCallback != null && debugCallback(current))
                yield return new WaitForSeconds(0.001f);

            tryCount++;
            if (Distance(current, goal) < WithinRangeRadius || tryCount >= tryCountMax)
            {
                Reconstruct(current, gridPoints);
                PathingCompleteCallback(true);
                yield break;
            }

            openSet.Remove(current);
            closedSet.Add(current, true);
            if(closedSetCallback != null)
            {
                closedSetCallback(current, true);
            }

            foreach (var neighbor in Neighbors(terrain, current))
            {
                if (closedSet.ContainsKey(neighbor))
                    continue;
                if (debugCallback != null && debugCallback(neighbor))
                {
                    yield return new WaitForSeconds(0.001f);
                }

                float projectedG = getGScore(current) + 1;

                if (!openSet.ContainsKey(neighbor))
                {
                    openSet[neighbor] = true;
                    if (openSetCallback != null)
                        openSetCallback(neighbor, true);
                }
                else if (projectedG >= getGScore(neighbor))
                    continue;

                //record it

                float h = Heuristic(neighbor, goal);
                if (abstractPathFinder.TryGetAbstractNodeAtPoint(neighbor, out var node) && (abstractPathFinder.TryGetDirectionHeuristic(node, out var dir) || abstractPathFinder.m_Goal == node))
                {
                    var dx = (float)(neighbor.X - current.X);
                    var dy = (float)(neighbor.Y - current.Y);
                    Vector2 b = new Vector2(dx, dy).normalized;
                    float suggestion = (1.0f - (Vector2.Dot(b, dir) + 1.0f)/2.0f);
                    if(node == abstractPathFinder.m_Goal)
                    {
                        suggestion = 0;
                    }
                    h *= 1.0f + (suggestion * 0.5f + abstractPathFinder.GetWeightForNode(node) * 0.5f);
                }

                if (!nodeLinks.ContainsKey(neighbor))
                    nodeLinks.Add(neighbor, current);
                else
                    nodeLinks[neighbor] = current;

                if (!gScore.ContainsKey(neighbor))
                    gScore.Add(neighbor, projectedG);
                else
                    gScore[neighbor] = projectedG;

                if(!fScore.ContainsKey(neighbor))
                    fScore.Add(neighbor, projectedG + h);
                else
                    fScore[neighbor] = projectedG + h;
            }
        }
        PathingCompleteCallback(false);
        yield return null;
    }

    private static float Distance(GridPoint x, GridPoint y)
    {
        return Vector2.Distance(new Vector2(x.X, x.Y), new Vector2(y.X, y.Y));
    }

    private float Heuristic(GridPoint start, GridPoint goal)
    {
        float dx = Mathf.Abs(start.X - goal.X);
        float dy = Mathf.Abs(start.Y - goal.Y);
        return D * (dx + dy) + (D2 - 2 * D) * Mathf.Min(dx, dy);
    }


    private float getGScore(GridPoint pt)
    {
        float score = int.MaxValue;
        gScore.TryGetValue(pt, out score);
        return score;
    }


    private float getFScore(GridPoint pt)
    {
        float score = int.MaxValue;
        fScore.TryGetValue(pt, out score);
        return score;
    }

    public IEnumerable<GridPoint> Neighbors(TerrainGenerator terrain, GridPoint center)
    {
        GridPoint pt = new GridPoint(center.X - GridStepSize, center.Y - GridStepSize);
        if (IsValidNeighbor(terrain, pt))
            yield return pt;

        pt = new GridPoint(center.X, center.Y - GridStepSize);
        if (IsValidNeighbor(terrain, pt))
            yield return pt;

        pt = new GridPoint(center.X + GridStepSize, center.Y - GridStepSize);
        if (IsValidNeighbor(terrain, pt))
            yield return pt;

        //middle row
        pt = new GridPoint(center.X - GridStepSize, center.Y);
        if (IsValidNeighbor(terrain, pt))
            yield return pt;

        pt = new GridPoint(center.X + GridStepSize, center.Y);
        if (IsValidNeighbor(terrain, pt))
            yield return pt;


        //bottom row
        pt = new GridPoint(center.X - GridStepSize, center.Y + GridStepSize);
        if (IsValidNeighbor(terrain, pt))
            yield return pt;

        pt = new GridPoint(center.X, center.Y + GridStepSize);
        if (IsValidNeighbor(terrain, pt))
            yield return pt;

        pt = new GridPoint(center.X + GridStepSize, center.Y + GridStepSize);
        if (IsValidNeighbor(terrain, pt))
            yield return pt;
    }

    public bool IsValidNeighbor(TerrainGenerator terrain, GridPoint pt)
    {
        int x = pt.X;
        int y = pt.Y;
        float normalizedHeight = terrain.GetHeightAtCoord(new Vector2(pt.X, pt.Y)) / terrain.MaxHeight;
        return normalizedHeight < settings.maxTraversableHeight && normalizedHeight > settings.minTraversableHeight;
    }

    private void Reconstruct(GridPoint current, Stack<GridPoint> points)
    {
        while (nodeLinks.ContainsKey(current))
        {
            points.Push(current);
            current = nodeLinks[current];
        }
    }

    private GridPoint nextBest()
    {
        float best = int.MaxValue;
        GridPoint bestPt = default;
        foreach (var node in openSet.Keys)
        {
            var score = getFScore(node);
            if (score < best)
            {
                bestPt = node;
                best = score;
            }
        }


        return bestPt;
    }
}

public struct GridPoint
{
    public int X, Y;
    public GridPoint(int x, int y) { X = x; Y = y; }
    public GridPoint(float x, float y) { X = (int)x; Y = (int)y;  }

    public GridPoint(Vector3 v3) { X = (int)v3.x; Y = (int)v3.z;  }
    public GridPoint(Vector2 v2) { X = (int)v2.x; Y = (int)v2.y; }

}
