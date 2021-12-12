using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractPathfindingNode
{
    HashSet<Vector2Int> entrances = new HashSet<Vector2Int>();

    // kinda debug only
    HashSet<Vector2Int> traversableTiles = new HashSet<Vector2Int>();
    public Vector2Int m_Coordinate;

    public AbstractPathfindingNode(bool[,] navMap, int startX, int startY, int endX, int endY, Vector2Int coordinate)
    {
        m_Coordinate = coordinate;
        int yOffset = (navMap.GetLength(0) - 1) * coordinate.y;
        int xOffset = (navMap.GetLength(1) - 1) * coordinate.x;
        Vector2Int offset = new Vector2Int(xOffset , yOffset);
        for (int y = startY; y < endY; y++)
        {
            if (navMap[startX, y])
                entrances.Add(offset + new Vector2Int(startX, y));
            if (navMap[endX - 1, y])
                entrances.Add(offset + new Vector2Int(endX - 1, y));
        }

        for (int x = startX; x < endX; x++)
        {
            if (navMap[x, startY])
                entrances.Add(offset + new Vector2Int(x,  startY));
            if (navMap[x, endY - 1])
                entrances.Add(offset + new Vector2Int(x, endY - 1 ));
        }

        //if (navMap[startX, startY])
        //    entrances.Add(coordinate + new Vector2Int(startX, startY));
        //if (navMap[startX, endY - 1])
        //    entrances.Add(coordinate + new Vector2Int(startX, endY - 1));
        //if (navMap[endX - 1, startY])
        //    entrances.Add(coordinate + new Vector2Int(endX - 1, startY));
        //if (navMap[endX - 1 , endY - 1])
        //    entrances.Add(coordinate + new Vector2Int(endX - 1, endY - 1 ));


        ///for debugging for now. should likely remove as we only really need the entrances
        
        for (int y = startY; y < endY; y++)
        {
            for(int x = startX; x < endX; x++)
            {
                if(navMap[x,y])
                    traversableTiles.Add(new Vector2Int(x, y));
            }
        }
    }
    public AbstractPathfindingNode(HashSet<Vector2Int> edges, HashSet<Vector2Int> validTiles, Vector2Int coordinate)
    {
        entrances = edges;
        traversableTiles = validTiles;
        m_Coordinate = coordinate;
    }

    public static AbstractPathfindingNode MergeNodes(AbstractPathfindingNode node1, AbstractPathfindingNode node2)
    {
        HashSet<Vector2Int> entrances = new HashSet<Vector2Int>(node1.entrances);
        entrances.UnionWith(node2.entrances);


        HashSet<Vector2Int> traversableTiles = new HashSet<Vector2Int>(node1.traversableTiles);
        traversableTiles.UnionWith(node2.traversableTiles);
        return new AbstractPathfindingNode(entrances, traversableTiles, node1.m_Coordinate);
    }

    public bool ContainsTraversableTile(Vector2Int vector2Int)
    {
        return traversableTiles.Contains(vector2Int);
    }

    public void TrimExcessInner(bool[,] navMap, Vector2Int coordinate)
    {
        entrances.IntersectWith(GetAllPotentialEntracnesForHashMap(navMap, coordinate));
    }

    public bool IsConnected(AbstractPathfindingNode neighbor)
    {
        return entrances.Overlaps(neighbor.entrances);
    }

    public HashSet<Vector2Int> GetEntrances()
    {
        return entrances;
    }

    public static HashSet<Vector2Int> GetAllPotentialEntracnesForHashMap(bool[,] navMap, Vector2Int coordinate)
    {
        int height = navMap.GetLength(0);
        int width = navMap.GetLength(1);
        int yOffset = (navMap.GetLength(0) - 1) * coordinate.y;
        int xOffset = (navMap.GetLength(1) - 1) * coordinate.x;
        Vector2Int offset = new Vector2Int(xOffset, yOffset);

        HashSet<Vector2Int> allPotentialEntrances = new HashSet<Vector2Int>();
        for (int y = 0; y < height; y++)
        {
            if (navMap[0, y])
                allPotentialEntrances.Add(offset + new Vector2Int(0, y));
            if (navMap[width - 1, y])
                allPotentialEntrances.Add(offset + new Vector2Int(width - 1, y));
        }

        for (int x = 0; x < width; x++)
        {
            if (navMap[x, 0])
                allPotentialEntrances.Add(offset + new Vector2Int(x, 0));
            if (navMap[x, height - 1])
                allPotentialEntrances.Add(offset + new Vector2Int(x, height - 1));
        }
        return allPotentialEntrances;
    }
}

public class MinimalAbstractPathFindingTree
{
    public AbstractPathfindingNode[] children { get; private set; }
    HashSet<AbstractPathfindingNode> AbstractNodes = new HashSet<AbstractPathfindingNode>();
    HashSet<Vector2Int> VisitedNodes = new HashSet<Vector2Int>();
    HashSet<Vector2Int> TraversableNodes = new HashSet<Vector2Int>();

    public static Color[] chunkColor = { Color.green, Color.cyan, Color.yellow, Color.magenta, Color.blue };
    private static int[] dir = {-1, 0, 1};
    private static Vector2Int WEST = Vector2Int.left;
    private static Vector2Int EAST = Vector2Int.right;
    private static Vector2Int NORTH = Vector2Int.up;
    private static Vector2Int SOUTH = Vector2Int.down;

    public MinimalAbstractPathFindingTree(bool[,] navMap, Vector2Int chunkCoordinate)
    {
        int width = navMap.GetLength(1);
        int height = navMap.GetLength(0);
        Stack<Vector2Int> OuterStack = new Stack<Vector2Int>();
        Stack<Vector2Int> inner = new Stack<Vector2Int>();

        // do this is at nav map generation time so we don't have to iterate through twice
        for (int j = 0; j < height ; j++)
        {
            for (int i = 0; i < width ; i++)
            {
                if(navMap[i,j])
                {
                    TraversableNodes.Add(new Vector2Int(i, j));
                    OuterStack.Push(new Vector2Int(i, j));
                }
            }
        }

        while (OuterStack.Count > 0)
        {
            Vector2Int current = OuterStack.Pop();
            if(!VisitedNodes.Contains(current))
            {
                VisitedNodes.Add(current);
                AbstractPathfindingNode node = new AbstractPathfindingNode(navMap, current.x, current.y, current.x + 1, current.y + 1, chunkCoordinate);
                inner.Push(current + NORTH);
                inner.Push(current + EAST);
                inner.Push(current + SOUTH);
                inner.Push(current + WEST);
                while (inner.Count > 0)
                {
                    Vector2Int neighbor = inner.Pop();
                    if (TraversableNodes.Contains(neighbor + NORTH) && !VisitedNodes.Contains(neighbor + NORTH))
                    {
                        Vector2Int nodeDir = neighbor + NORTH;
                        VisitedNodes.Add(nodeDir);
                        inner.Push(nodeDir);
                        AbstractPathfindingNode newNode = new AbstractPathfindingNode(navMap, nodeDir.x, nodeDir.y, nodeDir.x + 1, nodeDir.y + 1, chunkCoordinate);
                        node = AbstractPathfindingNode.MergeNodes(node, newNode);

                    }

                    if (TraversableNodes.Contains(neighbor + EAST) && !VisitedNodes.Contains(neighbor + EAST))
                    {
                        Vector2Int nodeDir = neighbor + EAST;
                        VisitedNodes.Add(nodeDir);
                        inner.Push(nodeDir);
                        AbstractPathfindingNode newNode = new AbstractPathfindingNode(navMap, nodeDir.x, nodeDir.y, nodeDir.x + 1, nodeDir.y + 1, chunkCoordinate);
                        node = AbstractPathfindingNode.MergeNodes(node, newNode);
                    }

                    if (TraversableNodes.Contains(neighbor + SOUTH) && !VisitedNodes.Contains(neighbor + SOUTH))
                    {
                        Vector2Int nodeDir = neighbor + SOUTH;
                        VisitedNodes.Add(nodeDir);
                        inner.Push(nodeDir);
                        AbstractPathfindingNode newNode = new AbstractPathfindingNode(navMap, nodeDir.x, nodeDir.y, nodeDir.x + 1, nodeDir.y + 1, chunkCoordinate);
                        node = AbstractPathfindingNode.MergeNodes(node, newNode);
                    }

                    if (TraversableNodes.Contains(neighbor + WEST) && !VisitedNodes.Contains(neighbor + WEST))
                    {
                        Vector2Int nodeDir = neighbor + WEST;
                        VisitedNodes.Add(nodeDir);
                        inner.Push(nodeDir);
                        AbstractPathfindingNode newNode = new AbstractPathfindingNode(navMap, nodeDir.x, nodeDir.y, nodeDir.x + 1, nodeDir.y + 1, chunkCoordinate);
                        node = AbstractPathfindingNode.MergeNodes(node, newNode);
                    }
                }
                AbstractNodes.Add(node);
            }

        }
        children = new AbstractPathfindingNode[AbstractNodes.Count];
        AbstractNodes.CopyTo(children);

        for (int i = 0; i < children.Length; i++)
        {
            children[i].TrimExcessInner(navMap,chunkCoordinate);
        }
    }

    public bool TryGetNodeContainingIndex(Vector2Int index, out AbstractPathfindingNode node)
    {
        node = null;
        foreach(var child in children)
        {
            if(child.ContainsTraversableTile(index))
            {
                node = child;
                return true;
            }
        }
        return false;
    }

}
