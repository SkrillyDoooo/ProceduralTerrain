using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Unit : WarpathObject, ISelectableObject
{

    private Vector3 TargetPosition;
    public float speed = 1.0f;
    private bool moving = false;
    public TerrainPathFinderSettings settings;
    // Update is called once per frame
    TerrainPathFinder pathFinder;
    AbstractPathfinder abstractPathFinder;

    Stack<GridPoint> gridPoints;
    Stack<GridPoint> refinedPoints;

    public bool debug;
    Dictionary<GridPoint, GameObject> debugNodes = new Dictionary<GridPoint, GameObject>();
    Dictionary<AbstractPathfindingNode, GameObject> abstractDebugNodes = new Dictionary<AbstractPathfindingNode, GameObject>();
    Dictionary<AbstractPathfindingNode, GameObject> directionDebugNodes = new Dictionary<AbstractPathfindingNode, GameObject>();


    GridPoint m_Start;
    GridPoint m_Goal;
    UnitUI unitUI;

    protected override void Start()
    {
        unitUI = GetComponent<UnitUI>();
        base.Start();
    }

    public void InitPathFinder()
    {
        pathFinder = new TerrainPathFinder(settings);
        gridPoints = new Stack<GridPoint>(1000);
        refinedPoints = new Stack<GridPoint>(1000);
        abstractPathFinder = new AbstractPathfinder(TerrainGenerator.Instance);
    }

    bool pathing = false;

    public void SetTargetPosition(Vector3 pose)
    {
        if(pathFinder == null)
            InitPathFinder();

        if (TerrainGenerator.Instance.GetHeightAtCoord(new Vector2(pose.x, pose.z))/TerrainGenerator.Instance.MaxHeight >= settings.maxTraversableHeight)
            return;

        m_Goal = new GridPoint(pose.x, pose.z);
        m_Start = new GridPoint(transform.position);

        if (pathing)
            StopAllCoroutines();

        gridPoints.Clear();
        CleanUpDebugObjects();
        debugNodes.Clear();
        StartCoroutine(abstractPathFinder.FindPath(m_Goal, m_Start, AbstractPathingComplete, AbstractDebugCallback, AbstractOpenSetCallback, AbstractClosedSetCallback, AbstractDirectionCallback));
        pathing = true;
    }

    private void AbstractClosedSetCallback(AbstractPathfindingNode node, bool b)
    {
        if (b)
        {
            ChangeColorOfAbstractDebugNode(node, Color.black);
        }
        else
        {
            ChangeColorOfAbstractDebugNode(node, Color.white);
        }
    }

    private void AbstractOpenSetCallback(AbstractPathfindingNode gp, bool b)
    {
        if (b)
        {
            ChangeColorOfAbstractDebugNode(gp, Color.black);
        }
        else
        {
            ChangeColorOfAbstractDebugNode(gp, Color.white);
        }
    }


    void ChangeColorOfAbstractDebugNode(AbstractPathfindingNode node, Color color)
    {
        if (abstractDebugNodes.ContainsKey(node))
        {
            abstractDebugNodes[node].GetComponent<Renderer>().material.color = color;
        }
    }

    private bool AbstractDebugCallback(AbstractPathfindingNode arg)
    {
        if (!abstractDebugNodes.ContainsKey(arg))
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            Vector3 worldCenter = new Vector3(arg.m_Coordinate.x * TerrainGenerator.Instance.meshSettings.meshWorldSize, TerrainGenerator.Instance.heightSettings.maxHeight, arg.m_Coordinate.y * TerrainGenerator.Instance.meshSettings.meshWorldSize);
            go.transform.localScale = Vector3.one * 4 * TerrainGenerator.Instance.meshSettings.meshScale;
            go.transform.position = worldCenter;
            abstractDebugNodes.Add(arg, go);
            return true;
        }
        return false;
    }

    void AbstractPathingComplete(bool success)
    {
        if(success)
        {
            Debug.Log("Abstract path found");
        }
        else
        {
            Debug.Log("Abstract path not found");
        }
        StartCoroutine(pathFinder.FindPath(TerrainGenerator.Instance, abstractPathFinder, m_Start, m_Goal, 4, gridPoints, PathingCompletedCallback, DebugCallback, OpenSetCallback, ClosedSetCallback));
    }

    private void AbstractDirectionCallback(AbstractPathfindingNode arg1, Vector2 arg2)
    {
        if (!directionDebugNodes.ContainsKey(arg1))
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            Vector3 worldCenter = new Vector3(arg1.m_Coordinate.x * TerrainGenerator.Instance.meshSettings.meshWorldSize, TerrainGenerator.Instance.heightSettings.maxHeight, arg1.m_Coordinate.y * TerrainGenerator.Instance.meshSettings.meshWorldSize);
            go.transform.localScale = new Vector3(2, 8, 2) * TerrainGenerator.Instance.meshSettings.meshScale;
            go.transform.position = worldCenter;
            go.transform.rotation = Quaternion.FromToRotation(go.transform.up, new Vector3(arg2.x, 0, arg2.y));
            go.transform.position += go.transform.up * 4 * TerrainGenerator.Instance.meshSettings.meshScale;

            directionDebugNodes.Add(arg1, go);
        }
    }

    void PathingCompletedCallback(bool success)
    {
        if(success)
        {
            gridPoints.Push(m_Start);
            UpdateDestination();
            moving = true;
        }
        else
        {
            Debug.Log("Path not found");
        }

        pathing = false;
    }

    bool DebugCallback(GridPoint current)
    {
        if(!debugNodes.ContainsKey(current))
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = Vector3.one * TerrainGenerator.Instance.meshSettings.meshScale;
            go.transform.position = new Vector3(current.X, TerrainGenerator.Instance.GetHeightAtCoord(new Vector2(current.X, current.Y)), current.Y);
            debugNodes.Add(current, go);
            return true;
        }
        return false;
    }

    void OpenSetCallback(GridPoint gp, bool b)
    {
        if(b)
        {
            ChangeColorOfDebugNode(gp, Color.blue);
        }
        else
        {
            ChangeColorOfDebugNode(gp, Color.white);
        }
    }

    void ClosedSetCallback(GridPoint gp, bool b)
    {
        if (b)
        {
            ChangeColorOfDebugNode(gp, Color.black);
        }
        else
        {
            ChangeColorOfDebugNode(gp, Color.white);
        }
    }

    void ChangeColorOfDebugNode(GridPoint gp, Color color)
    {
        if(debugNodes.ContainsKey(gp))
        {
            debugNodes[gp].GetComponent<Renderer>().material.color = color;
        }
    }


    public void CleanUpDebugObjects()
    {
        foreach (var kvp in debugNodes)
        {
            GameObject.Destroy(kvp.Value);
        }
        debugNodes.Clear();

        foreach (var kvp in abstractDebugNodes)
        {
            GameObject.Destroy(kvp.Value);
        }
        abstractDebugNodes.Clear();

        foreach(var kvp in directionDebugNodes)
        {
            GameObject.Destroy(kvp.Value);
        }
        directionDebugNodes.Clear();
    }

    void Update()
    {
        if (!moving)
            return;
        UpdateTargetHeight();
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, Time.deltaTime * speed);
        if (transform.position == TargetPosition)
        {
            if(gridPoints.Count > 0)
            {
                UpdateDestination();
            }
            else
            {
                moving = false;
            }
        }
    }

    private void UpdateTargetHeight()
    {
        TargetPosition.y = TerrainGenerator.Instance.GetHeightAtCoord(new Vector2(transform.position.x, transform.position.z)) + 1;
        Vector3 v = transform.position;
        v.y = TargetPosition.y ;
        transform.position = v ;
    }

    void UpdateDestination()
    {
        GridPoint gridPoint = gridPoints.Pop();
        TargetPosition = new Vector3(gridPoint.X, 0, gridPoint.Y);
    }

    public override ContextPanelData GetContextPanelData()
    {
        return unitUI.GetContextPanelData();
    }

    public void SetInfoPanelRoot(VisualElement root)
    {
        unitUI.SetInfoPanelRoot(root);
    }

    public override void Deselect()
    {
        base.Deselect();
    }

    public override void ContextButtonPressed(int index)
    {
        throw new NotImplementedException();
    }

    public override void CommandButtonPressed(int buttonId)
    {
        throw new NotImplementedException();
    }

    public override void Select(VisualElement root)
    {
        base.Select(root);
    }

    public override void RightClick(Vector3 rightClickPoint)
    {
        SetTargetPosition(rightClickPoint);
    }

    protected override void DestroyWarpathObject()
    {
        RemoveObjectFromSelectionList();
        Destroy(gameObject, 0.5f);
        PlayerManifest.Instance.RemoveUnit(transform);
    }
}
