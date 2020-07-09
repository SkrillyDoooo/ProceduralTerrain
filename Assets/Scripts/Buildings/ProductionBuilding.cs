using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionBuilding : MonoBehaviour
{
    Vector3 rallyPoint;

    private Queue<Unit> productionQueue = new Queue<Unit>(5);

    public event System.Action UnitComplete;

    public int productionQueueCount
    {
        get { return productionQueue.Count; }
    }

    public float currentProgress
    {
        get { return timer / currentTimeToComplete; }
    }

    [SerializeField]
    private Unit[] productionOptions;

    float currentTimeToComplete = 1.0f;
    float timer;
    // Update is called once per frame
    void Update()
    {
        if (productionQueueCount == 0)
            return;
        timer += Time.deltaTime;
        if(timer > currentTimeToComplete)
        {
            ItemComplete();
        }
    }

    public Unit[] GetCurrentProductionArray()
    {
        return productionQueue.ToArray();
    }

    public bool TryAddItem(int optionIndex)
    {
        Unit option = productionOptions[optionIndex];
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
        Unit completedItem = productionQueue.Dequeue();
        UnitComplete();
        var go = Instantiate(completedItem.prefab,transform.position,Quaternion.identity) as GameObject;
        var unit = go.GetComponent<PathfindingUnit>();
        unit.SetTargetPosition(transform.position + Vector3.right * 35);
        go.name = completedItem.name;
        StartNextUnit();
    }

    public void RemoveItem(int index)
    {
        Unit[] tmp = productionQueue.ToArray();
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

    internal void Demolish()
    {
        while(productionQueue.Count > 0)
        {
            var unit = productionQueue.Dequeue();
            Game.Instance.Refund(unit.cost);
        }
    }
}


[System.Serializable]
public struct Unit
{
    public string name;
    public GameObject prefab;
    public float timeToCreate;
    public Texture2D icon;
    public ResourceManager.ItemCost[] cost;
}

