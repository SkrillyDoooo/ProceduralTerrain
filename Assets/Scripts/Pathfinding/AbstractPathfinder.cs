using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractPathfinder 
{

    Dictionary<AbstractPathfindingNode, bool> closedSet = new Dictionary<AbstractPathfindingNode, bool>();
    Dictionary<AbstractPathfindingNode, bool> openSet = new Dictionary<AbstractPathfindingNode, bool>();

    //cost of start to this key node
    Dictionary<AbstractPathfindingNode, float> gScore = new Dictionary<AbstractPathfindingNode, float>();
    //cost of start to goal, passing through key node
    Dictionary<AbstractPathfindingNode, float> fScore = new Dictionary<AbstractPathfindingNode, float>();

    public Dictionary<AbstractPathfindingNode, AbstractPathfindingNode> nodeLinks = new Dictionary<AbstractPathfindingNode, AbstractPathfindingNode>();
    public Dictionary<AbstractPathfindingNode, int> path = new Dictionary<AbstractPathfindingNode, int>();
    public AbstractPathfindingNode m_Goal;
    private TerrainGenerator m_Terrain;
    const float D = 1;
    const float D2 = 1.41421356237f;

    public AbstractPathfinder(TerrainGenerator terrain)
    {
        m_Terrain = terrain;
    }

    public bool TryGetAbstractNodeAtPoint(GridPoint point, out AbstractPathfindingNode node)
    {
        if (m_Terrain.TryGetNavMapAtWorldPoint(new Vector2(point.X, point.Y), out var navMapStart, out var startIndex))
        {
            if (navMapStart.abstractPathFindingTree.TryGetNodeContainingIndex(startIndex, out node))

            {
                return true;
            }
        }
        node = null;
        return false;
    }

    public IEnumerator FindPath(GridPoint startPoint, GridPoint goalPoint, Action<bool> PathingCompleteCallback, Func<AbstractPathfindingNode, bool> debugCallback = null, Action<AbstractPathfindingNode, bool> openSetCallback = null, Action<AbstractPathfindingNode, bool> closedSetCallback = null, Action<AbstractPathfindingNode, Vector2> abtractDirectionNodeCallback = null)
    {
        if (TryGetAbstractNodeAtPoint(startPoint, out var start) && TryGetAbstractNodeAtPoint(goalPoint, out var goal))
        {

            // TODO:: make search resumeable 
            closedSet.Clear();
            openSet.Clear();
            gScore.Clear();
            fScore.Clear();
            nodeLinks.Clear();

            openSet[start] = true;
            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal);
            m_Goal = start;

            while (openSet.Count > 0)
            {
                var current = nextBest();

                if (current == goal)
                {
                    Reconstruct(current);
                    PathingCompleteCallback(true);
                    yield break;
                }

                openSet.Remove(current);

                closedSet.Add(current, true);
                if (closedSetCallback != null)
                {
                    closedSetCallback(current, true);
                }


                foreach (var neighbor in Neighbors(m_Terrain, current))
                {
                    if (closedSet.ContainsKey(neighbor))
                        continue;

                    if (debugCallback != null && debugCallback(neighbor))
                    {
                        yield return new WaitForSeconds(0.001f);
                    }

                    var projectedG = getGScore(current) + 1;

                    if (!openSet.ContainsKey(neighbor))
                    {
                        openSet[neighbor] = true;
                        if (openSetCallback != null)
                            openSetCallback(neighbor, true);
                    }
                    else if (projectedG >= getGScore(neighbor))
                        continue;


                    float h = Heuristic(neighbor, goal);

                    //record it
                    if (!nodeLinks.ContainsKey(neighbor))
                    {
                        nodeLinks.Add(neighbor, current);
                        if (abtractDirectionNodeCallback != null && TryGetDirectionHeuristic(neighbor, out var dir))
                        {
                            abtractDirectionNodeCallback(neighbor, dir);
                            yield return new WaitForSeconds(0.001f);
                        }
                    }
                    else
                        nodeLinks[neighbor] = current;

                    if (!gScore.ContainsKey(neighbor))
                        gScore.Add(neighbor, projectedG);
                    else
                        gScore[neighbor] = projectedG;

                    if (!fScore.ContainsKey(neighbor))
                        fScore.Add(neighbor, projectedG + h);
                    else
                        fScore[neighbor] = projectedG + h;
                }
            }

            PathingCompleteCallback(false);
        }
    }

    private void Reconstruct(AbstractPathfindingNode current)
    {
        path.Clear();
        int step = 0;
        while (nodeLinks.ContainsKey(current))
        {
            path.Add(current, step++);
            current = nodeLinks[current];
        }
        path.Add(m_Goal, step);
    }

    public float GetWeightForNode(AbstractPathfindingNode node)
    {
        if(path.ContainsKey(node))
        {
            return (1.0f - (float)path[node]/(float)path.Count);
        }
        return 0.0f;
    }

    public bool TryGetDirectionHeuristic(AbstractPathfindingNode node, out Vector2 dir)
    {
        if(nodeLinks.ContainsKey(node))
        {
            AbstractPathfindingNode link = nodeLinks[node];
            var dx = link.m_Coordinate.x - node.m_Coordinate.x;
            var dy = link.m_Coordinate.y - node.m_Coordinate.y;
            dir = new Vector2(dx, dy).normalized;
            return true;
        }
        dir = Vector2.zero;
        return false;
        // else resume search
    }

    private float Heuristic(AbstractPathfindingNode start, AbstractPathfindingNode goal)
    {
        float dx = Mathf.Abs(start.m_Coordinate.x - goal.m_Coordinate.x);
        float dy = Mathf.Abs(start.m_Coordinate.y - goal.m_Coordinate.y);
        return D * (dx + dy) + (D2 - 2 * D) * Mathf.Min(dx, dy);
    }

    private float getGScore(AbstractPathfindingNode pt)
    {
        float score = int.MaxValue;
        gScore.TryGetValue(pt, out score);
        return score;
    }


    private float getFScore(AbstractPathfindingNode pt)
    {
        float score = int.MaxValue;
        fScore.TryGetValue(pt, out score);
        return score;
    }

    public IEnumerable<AbstractPathfindingNode> Neighbors(TerrainGenerator terrain, AbstractPathfindingNode current)
    {
        NavMap nav = default;
        if(terrain.TryGetNavMapAtCoordinate(current.m_Coordinate + Vector2.up, out nav))
        {
            foreach(AbstractPathfindingNode node in nav.abstractPathFindingTree.children)
            {
                if (node.IsConnected(current))
                    yield return node;
            }
        }
        if (terrain.TryGetNavMapAtCoordinate(current.m_Coordinate + Vector2.right, out nav))
        {
            foreach (AbstractPathfindingNode node in nav.abstractPathFindingTree.children)
            {
                if (node.IsConnected(current))
                    yield return node;
            }
        }
        if (terrain.TryGetNavMapAtCoordinate(current.m_Coordinate + Vector2.left, out nav))
        {
            foreach (AbstractPathfindingNode node in nav.abstractPathFindingTree.children)
            {
                if (node.IsConnected(current))
                    yield return node;
            }
        }
        if (terrain.TryGetNavMapAtCoordinate(current.m_Coordinate + Vector2.down, out nav))
        {
            foreach (AbstractPathfindingNode node in nav.abstractPathFindingTree.children)
            {
                if (node.IsConnected(current))
                    yield return node;
            }
        }

        if (terrain.TryGetNavMapAtCoordinate(current.m_Coordinate + Vector2.down + Vector2.right, out nav))
        {
            foreach (AbstractPathfindingNode node in nav.abstractPathFindingTree.children)
            {
                if (node.IsConnected(current))
                    yield return node;
            }
        }

        if (terrain.TryGetNavMapAtCoordinate(current.m_Coordinate + Vector2.down + Vector2.left, out nav))
        {
            foreach (AbstractPathfindingNode node in nav.abstractPathFindingTree.children)
            {
                if (node.IsConnected(current))
                    yield return node;
            }
        }

        if (terrain.TryGetNavMapAtCoordinate(current.m_Coordinate + Vector2.up + Vector2.right, out nav))
        {
            foreach (AbstractPathfindingNode node in nav.abstractPathFindingTree.children)
            {
                if (node.IsConnected(current))
                    yield return node;
            }
        }

        if (terrain.TryGetNavMapAtCoordinate(current.m_Coordinate + Vector2.up + Vector2.left, out nav))
        {
            foreach (AbstractPathfindingNode node in nav.abstractPathFindingTree.children)
            {
                if (node.IsConnected(current))
                    yield return node;
            }
        }
    }
    public bool IsValidNeighbor(TerrainGenerator terrain, AbstractPathfindingNode pt, AbstractPathfindingNode current)
    {
        return current.IsConnected(pt);
    }

    private AbstractPathfindingNode nextBest()
    {
        float best = int.MaxValue;
        AbstractPathfindingNode bestPt = null;
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
