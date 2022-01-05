using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public class NavigationNodePool 
{
    ConcurrentDictionary<NavigationNodeID, NavigationNode> nodes = new ConcurrentDictionary<NavigationNodeID, NavigationNode>();


    public ConcurrentDictionary<NavigationNodeID, NavigationNode> GetNodes()
    {
        return nodes;
    }

    public NavigationNode AcquireNavigationNode(TerrainGridGenerator.TerrainCellType type, NavigationNodeID id, Vector2Int localCoord, Vector2Int worldChunkCoord)
    {
        NavigationNode node;
        if(!nodes.TryGetValue(id, out node))
        {   
            //TODO:: actually pool navigation nodes
            node = new NavigationNode(type,id,localCoord, worldChunkCoord);
            nodes.TryAdd(id, node);
        }
        return node;
    }

    public bool TryGetNode(NavigationNodeID id, out NavigationNode node)
    {
        return nodes.TryGetValue(id, out node);
    }

    public void ReleaseNavigationNode(NavigationNodeID id)
    {
        //add Navigation node back to inactive nodes
        nodes.TryRemove(id, out var removed);
    }
}

public static class NavigationNodeIDExtensions
{
   public static NavigationNodeID Up(this NavigationNodeID id)
    {
        return new NavigationNodeID { coordinate = id.coordinate + Vector2Int.up, level = id.level};
    }

    public static NavigationNodeID Down(this NavigationNodeID id)
    {
        return new NavigationNodeID { coordinate = id.coordinate + Vector2Int.down, level = id.level};
    }

    public static NavigationNodeID Left(this NavigationNodeID id)
    {
        return new NavigationNodeID { coordinate = id.coordinate + Vector2Int.left, level = id.level};
    }

    public static NavigationNodeID Right(this NavigationNodeID id)
    {
        return new NavigationNodeID { coordinate = id.coordinate + Vector2Int.right, level = id.level};
    }

    public static NavigationNodeID TopLeftChild(this NavigationNodeID id)
    {
        return new NavigationNodeID { coordinate = id.coordinate + Vector2Int.up, level = id.level};
    }

    public static NavigationNodeID TopRightChild(this NavigationNodeID id)
    {
        return new NavigationNodeID { coordinate = id.coordinate + Vector2Int.up + Vector2Int.right, level = id.level - 1};
    }

    public static NavigationNodeID BottomLeftChild(this NavigationNodeID id)
    {
        return new NavigationNodeID { coordinate = id.coordinate, level = id.level - 1};
    }

    public static NavigationNodeID BottomRightChild(this NavigationNodeID id)
    {
        return new NavigationNodeID { coordinate = id.coordinate + Vector2Int.right, level = id.level - 1};
    }
}

public struct NavigationNodeID
{
    public Vector2Int coordinate;
    public int level;

    public NavigationNodeID Up()
    {
        return new NavigationNodeID { coordinate = coordinate + Vector2Int.up, level = level};
    }

    public NavigationNodeID Down()
    {
        return new NavigationNodeID { coordinate = coordinate + Vector2Int.down, level = level};
    }

    public NavigationNodeID Left()
    {
        return new NavigationNodeID { coordinate = coordinate + Vector2Int.left, level = level};
    }

    public NavigationNodeID Right()
    {
        return new NavigationNodeID { coordinate = coordinate + Vector2Int.right, level = level};
    }

    public NavigationNodeID TopLeftChild()
    {
        return new NavigationNodeID { coordinate = coordinate * 2 + Vector2Int.up, level = level - 1};
    }

    public NavigationNodeID TopRightChild()
    {
        return new NavigationNodeID { coordinate = coordinate * 2 + Vector2Int.up + Vector2Int.right, level = level - 1};
    }

    public NavigationNodeID BottomLeftChild()
    {
        return new NavigationNodeID { coordinate = coordinate * 2, level = level - 1};
    }

    public NavigationNodeID BottomRightChild()
    {
        return new NavigationNodeID { coordinate = coordinate * 2 + Vector2Int.right, level = level - 1};
    }
}

public class NavigationNode
{
    public enum TransitionType
    {
        None = 0,
        Land = 1,
        Water = 2,
    }

    private Vector2Int localxy;
    private Vector2Int worldxy;

    NavigationNode m_RepresentativeNode;
    List<NavigationNode> m_Neighbors = new List<NavigationNode>();
    List<TransitionType> m_TransitionType = new List<TransitionType>();
    TerrainGridGenerator.TerrainCellType m_TerrainType;
    public NavigationNodeID m_Parent;
    List<NavigationNode> m_Children = new List<NavigationNode>();

    public NavigationNodeID m_id {get; private set;}

    public NavigationNode(TerrainGridGenerator.TerrainCellType type, NavigationNodeID id, Vector2Int local, Vector2Int world)
    {
        m_TerrainType = type;
        m_id = id;
        localxy = local;
        worldxy = world;
    }

    public List<NavigationNode> GetNeighbors()
    {
        return m_Neighbors;
    }

    public static NavigationNode BuildTree(TerrainGridGenerator.TerrainCellType[,] values, int dimensions, Vector2Int chunkcoord, NavigationNodePool nodePool, int maxLevel)
    {
        NavigationNode node = null;
        List<NavigationNode> parentNeighborsList = new List<NavigationNode>();
        List<NavigationNode> childrenToAddList = new List<NavigationNode>();

        for(int level = 0; level < maxLevel; level++)
        {
            int dimension = dimensions/(int)Mathf.Pow(2, level);
            Vector2Int worldChunkCoord = chunkcoord * dimension;
            for (int x = 0; x < dimension; x++)
            {
                for(int y = 0; y < dimension; y++)
                {
                    TerrainGridGenerator.TerrainCellType value = values[x,dimension - y - 1];
                    if(level == 0 && (value & TerrainGridGenerator.TerrainCellType.Land) == 0)
                    {
                        continue;
                    }
                    Vector2Int localCoordinate = new Vector2Int(x, y);
                    NavigationNodeID nodeId = new NavigationNodeID{coordinate = localCoordinate + worldChunkCoord, level = level};
                    NavigationNodeID parentId = new NavigationNodeID{coordinate = new Vector2Int(nodeId.coordinate.x >> 1, nodeId.coordinate.y >> 1), level = level+1};
                    if(level == 0)
                    {
                        node = nodePool.AcquireNavigationNode(value, nodeId, localCoordinate, worldChunkCoord);
                        node.SetParent(parentId);
                        //set permissions of movement? Maybe there are boats or amphibuous units?
                        if(nodePool.TryGetNode(nodeId.Left(), out var left) && left.m_TerrainType == node.m_TerrainType)
                            node.AddNeighbor(left);
                        if(nodePool.TryGetNode(nodeId.Right(), out var right) && right.m_TerrainType == node.m_TerrainType)
                            node.AddNeighbor(right);
                        if(nodePool.TryGetNode(nodeId.Up(), out var up) && up.m_TerrainType == node.m_TerrainType)
                            node.AddNeighbor(up);
                        if(nodePool.TryGetNode(nodeId.Down(), out var down) && down.m_TerrainType == node.m_TerrainType)
                            node.AddNeighbor(down);
                    }
                    else if(level > 0)
                    {
                        bool interConnected = false;
                        interConnected |= GetChildData(nodeId.BottomLeftChild(), childrenToAddList, parentNeighborsList, nodePool);
                        interConnected |= GetChildData(nodeId.BottomRightChild(), childrenToAddList, parentNeighborsList, nodePool);
                        interConnected |= GetChildData(nodeId.TopLeftChild(), childrenToAddList, parentNeighborsList, nodePool);
                        interConnected |= GetChildData(nodeId.TopRightChild(), childrenToAddList, parentNeighborsList, nodePool);

                        if(childrenToAddList.Count > 0 && interConnected)
                        {
                            node = nodePool.AcquireNavigationNode(TerrainGridGenerator.TerrainCellType.None, nodeId, localCoordinate, worldChunkCoord);
                            node.SetParent(parentId);
                            node.AddNeighbors(parentNeighborsList);
                            node.AddChildren(childrenToAddList);
                        }
                        childrenToAddList.Clear();
                        parentNeighborsList.Clear();
                    }
                }
            }
        }

        return node;
    }

    public static bool GetChildData(NavigationNodeID childID, List<NavigationNode> childNodesToAdd, List<NavigationNode> neighborsToAdd, NavigationNodePool nodePool) 
    {
        bool isInterConnected = false;
        if(nodePool.TryGetNode(childID, out var childNode))
        {
            childNodesToAdd.Add(childNode);
            childNode.GetNeighborData(nodePool, neighborsToAdd, out bool horizontalConnection, out bool verticalConnection);
            isInterConnected = horizontalConnection && verticalConnection;
        }
        return isInterConnected;
    }

    public void GetNeighborData(NavigationNodePool navPool, List<NavigationNode> nodes, out bool horizontalConnection, out bool verticalConnection)
    {
        horizontalConnection = false;
        verticalConnection = false;
        foreach(var neighbor in m_Neighbors)
        {
            // may need to acquire a node here if it doesn't exist.
            if(!neighbor.m_Parent.Equals(m_Parent) && navPool.TryGetNode(neighbor.m_Parent, out var parent))
            {
                nodes.Add(parent);
            }
            else if(neighbor.m_Parent.Equals(m_Parent))
            {
                if(neighbor.IsHorizontalNeighbor(m_id))
                {
                    horizontalConnection = true;
                }

                if(neighbor.IsVerticalNeighbor(m_id))
                {
                    verticalConnection = true;
                }
            }
        }
    }

    public void SetParent(NavigationNodeID parent)
    {
        m_Parent = parent;
    }

    public void AddChild(NavigationNode child)
    {
        if(!m_Children.Contains(child))
        {
            m_Children.Add(child);
        }
    }

    public void AddChildren(List<NavigationNode> children)
    {
        foreach(var child in children)
        {
            AddChild(child);
        }
    }

    public void AddNeighbor(NavigationNode node)
    {
        if(!m_Neighbors.Contains(node))
        {
            m_Neighbors.Add(node);
            node.AddNeighbor(this);
        }
    }

    public void AddNeighbors(List<NavigationNode> nodes)
    {
        foreach(var node in nodes)
        {
            AddNeighbor(node);
        }
    }

    public bool IsHorizontalNeighbor(NavigationNodeID id)
    {
        return m_id.coordinate.y == id.coordinate.y;
    }

    public bool IsVerticalNeighbor(NavigationNodeID id)
    {
        return m_id.coordinate.x == id.coordinate.x;
    }

    public Vector3 GetPositionFromNodeCoordinate(float worldScale, int chunkDimensions)
    {
        int power = 1 << m_id.level;
        Vector2 world = new Vector2(worldxy.x, worldxy.y);
        Vector3 topLeft = new Vector3(-1, 0, -1) * chunkDimensions/2;
        Vector2 local = (world + new Vector2((float)localxy.x + 0.5f, (float)localxy.y + 0.5f)) * power;
        return new Vector3(local.x, 0,  local.y + 1) + topLeft;
    }

}
