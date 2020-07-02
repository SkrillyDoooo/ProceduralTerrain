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

    float currentTimeToComplete;
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

    public void AddItem(int optionIndex)
    {
        Unit option = productionOptions[optionIndex];
        if (productionQueueCount == 0)
            currentTimeToComplete = option.timeToCreate;

        productionQueue.Enqueue(option);
    }

    private void ItemComplete()
    {
        Unit completedItem = productionQueue.Dequeue();
        UnitComplete();
        var go = Instantiate(completedItem.prefab) as GameObject;
        go.name = completedItem.name;
        StartNextUnit();
    }

    public void RemoveItem(int index)
    {
        Unit[] tmp = productionQueue.ToArray();
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
        if (productionQueueCount == 0)
            return;
        currentTimeToComplete = productionQueue.Peek().timeToCreate;
        timer = 0;
    }
}


[System.Serializable]
public struct Unit
{
    public string name;
    public GameObject prefab;
    public float timeToCreate;
    public Texture2D icon;
}

