using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class ProductionBuilding : WarpathObject, ISelectableObject
{
    Vector3 rallyPoint;
    const int maxProduction = 5;
    private Queue<UnitInfo> productionQueue = new Queue<UnitInfo>(maxProduction);

    private Renderer rallyPointRenderer;
    private ProductionBuildingUI ui;


    public int productionQueueCount
    {
        get { return productionQueue.Count; }
    }

    public float currentProgress
    {
        get { return timer / currentTimeToComplete; }
    }

    public int CurrentHealth => throw new NotImplementedException();

    public int MaxHealth => throw new NotImplementedException();

    [SerializeField]
    private UnitInfo[] productionOptions;

    protected override void Start()
    {
        ui = GetComponent<ProductionBuildingUI>();
        ui.CancelItem += CancelItem;

        rallyPoint = transform.position + transform.forward * -35;
        rallyPointRenderer = GameObject.CreatePrimitive(PrimitiveType.Cylinder).GetComponent<Renderer>();
        rallyPointRenderer.transform.localScale = Vector3.one + Vector3.up * 30;
        rallyPointRenderer.transform.parent = transform;
        rallyPointRenderer.transform.position = rallyPoint;
        rallyPointRenderer.enabled = false;

        base.Start();
    }

    private void CancelItem(int index)
    {
        if (index < productionQueueCount && productionQueueCount != 0)
        {
            RemoveItem(index);
            ui.UpdateProductionButtons(productionQueue.ToArray());
        }
    }   

    float currentTimeToComplete = 1.0f;
    float timer;
    // Update is called once per frame
    void Update()
    {
        if(selected)
        {
            ui.UpdateProgress(currentProgress);
        }

        if (productionQueueCount == 0)
            return;
        timer += Time.deltaTime;
        if(timer > currentTimeToComplete)
        {
            ItemComplete();
        }
    }

    public UnitInfo[] GetCurrentProductionArray()
    {
        return productionQueue.ToArray();
    }

    public bool TryAddItem(int optionIndex)
    {
        UnitInfo option = productionOptions[optionIndex];
        if (!Game.Instance.TryPurchase(option.cost))
            return false;

        if (productionQueueCount == 0)
            currentTimeToComplete = option.timeToCreate;

        productionQueue.Enqueue(option);
        return true;
    }

    private void ItemComplete()
    {
        if (productionQueue.Count == 0)
            return;
        UnitInfo completedItem = productionQueue.Dequeue();
        ui.UpdateProductionButtons(productionQueue.ToArray());
        var go = Instantiate(completedItem.prefab,transform.position,Quaternion.identity) as GameObject;
        PlayerManifest.Instance.AddUnit(go.transform);

        var unit = go.GetComponent<Unit>();
        unit.SetTargetPosition(rallyPoint);
        go.name = completedItem.name;
        StartNextUnit();
    }

    public void RemoveItem(int index)
    {
        UnitInfo[] tmp = productionQueue.ToArray();
        Game.Instance.Refund(tmp[index].cost);
        int queueCount = productionQueue.Count;
        productionQueue.Clear();
        int i = 0;
        for (i = 0; i < queueCount; i++)
        {
            if(i != index)
                productionQueue.Enqueue(tmp[i]);
        }
        if (index == 0)
        {
            StartNextUnit();
        }
    }

    void StartNextUnit()
    {
        timer = 0;
        if (productionQueueCount == 0)
            return;
        currentTimeToComplete = productionQueue.Peek().timeToCreate;
    }

    protected override void DestroyWarpathObject()
    {
        while(productionQueue.Count > 0)
        {
            var unit = productionQueue.Dequeue();
            Game.Instance.Refund(unit.cost);
        }

        RemoveObjectFromSelectionList();
        PlayerManifest.Instance.RemoveBuilding(transform);
        Destroy(gameObject, 0.2f);
    }

    internal void SetRallyPoint(Vector3 rightClickPoint)
    {
        rallyPoint = rightClickPoint;
        rallyPointRenderer.transform.position = rallyPoint;
    }

    public override ContextPanelData GetContextPanelData()
    {
        return ui.GetContextPanelData();
    }

    public override void Deselect()
    {
        ui.Deselect();
        rallyPointRenderer.enabled = false;
        base.Deselect();
    }

    public override void ContextButtonPressed(int index)
    {
        if (productionQueueCount >= maxProduction)
            return;

        if (!TryAddItem(index))
            return;

        ui.UpdateProductionButtons(productionQueue.ToArray());
    }


    public override void CommandButtonPressed(int buttonId)
    {
        switch (buttonId)
        {
            case ((int)ProductionBuildingUI.CommandButtonIds.Destroy):
                DestroyWarpathObject();
                break;
        }
    }

    public override void Select(VisualElement root)
    {
        ui.Select(root, productionQueue.ToArray(), currentProgress);
        rallyPointRenderer.enabled = true;
        base.Select(root);
    }

    public override void RightClick(Vector3 rightClickPoint)
    {
        SetRallyPoint(rightClickPoint);
    }
}


[System.Serializable]
public struct UnitInfo
{
    public string name;
    public GameObject prefab;
    public float timeToCreate;
    public Texture2D icon;
    public ResourceManager.ItemCost[] cost;
}

