using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class PathFindingTests
{
    // A Test behaves as an ordinary method
    [Test]
    public void AbstractNodeConnections()
    {
        bool[,] b = new bool[8, 8];
        for(int i = 0; i < 8; i++)
        {
            for(int j = 0; j < 8; j++)
            {
                b[i, j] = true;
            }
        }
        Vector2Int coord = Vector2Int.zero;
        AbstractPathfindingNode node1 = new AbstractPathfindingNode(b, 0, 0, 5, 4, coord);
        AbstractPathfindingNode node2 = new AbstractPathfindingNode(b, 4, 0, 8, 4, coord);
        AbstractPathfindingNode node3 = new AbstractPathfindingNode(b, 0, 4, 4, 8, coord);
        AbstractPathfindingNode node4 = new AbstractPathfindingNode(b, 4, 4, 8, 8, coord);

        Assert.True(node1.IsConnected(node2));
        Assert.IsFalse(node3.IsConnected(node4));
    }

    [Test]
    public void MergeAbstractNodeConnections()
    {
        bool[,] b = new bool[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                b[i, j] = true;
            }
        }
        Vector2Int coord = Vector2Int.zero;
        AbstractPathfindingNode node1 = new AbstractPathfindingNode(b, 0, 0, 3, 4, coord);
        AbstractPathfindingNode node2 = new AbstractPathfindingNode(b, 2, 0, 4, 4, coord);

        Assert.True(node1.IsConnected(node2));
        if (node1.IsConnected(node2))
        {
            AbstractPathfindingNode node = AbstractPathfindingNode.MergeNodes(node1, node2);
            node.TrimExcessInner(b, coord);
            HashSet<Vector2Int> entrances = node.GetEntrances();
            HashSet<Vector2Int> potentialEntrances = AbstractPathfindingNode.GetAllPotentialEntracnesForHashMap(b, coord);

            foreach (Vector2Int v in potentialEntrances)
            {
                Assert.True(entrances.Contains(v));
            }

            foreach (Vector2Int v in entrances)
            {
                Assert.True(potentialEntrances.Contains(v));
            }
        }
    }

    [Test]
    public void ConstructMinimalAbstractTree()
    {
        bool[,] b = new bool[,]
        {
            {true, true, true, true },
            {false, false, false, true },
            {false, false, false, true },
            {true, true, false, true}
        };

        MinimalAbstractPathFindingTree tree = new MinimalAbstractPathFindingTree(b, Vector2Int.zero);
        Assert.AreEqual(2, tree.children.Length);
    }

    [Test]
    public void ConstructMinimalAbstractTreeThree()
    {
        bool[,] b = new bool[,]
        {
            {true, true, true, true, true, true},
            {false, false, false, true, true, true},
            {true, true, false, true, true, true },
            {false, false, false, true, true, true},
            {true, false, false, true, true, true},
            {true, true, false, true, true, true},
        };

        MinimalAbstractPathFindingTree tree = new MinimalAbstractPathFindingTree(b, Vector2Int.zero);
        Assert.AreEqual(3, tree.children.Length);
    }
}
