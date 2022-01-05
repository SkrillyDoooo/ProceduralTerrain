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
    Stack<GridPoint> gridPoints;
    Stack<GridPoint> refinedPoints;

    public bool debug;
    Dictionary<GridPoint, GameObject> debugNodes = new Dictionary<GridPoint, GameObject>();

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
        gridPoints = new Stack<GridPoint>(1000);
        refinedPoints = new Stack<GridPoint>(1000);
    }

    bool pathing = false;

    public void SetTargetPosition(Vector3 pose)
    {
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
