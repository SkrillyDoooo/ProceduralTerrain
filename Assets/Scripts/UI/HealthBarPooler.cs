using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarPooler
{
    private VisualElement healthBarRoot;
    private VisualTreeAsset healthBarAsset;

    const int poolCapactiy = 100;
    Queue<int> indexAvailabilityQueue = new Queue<int>(poolCapactiy);
    VisualElement[] healthBarComponents = new VisualElement[poolCapactiy];

    public HealthBarPooler(VisualElement healthBarRoot, VisualTreeAsset healthBarAsset)
    {
        this.healthBarRoot = healthBarRoot;
        this.healthBarAsset = healthBarAsset;
        for(int i = 0; i < poolCapactiy; i++)
        {
            healthBarAsset.CloneTree(healthBarRoot);
            healthBarComponents[i] = healthBarRoot.ElementAt(i);
            healthBarComponents[i].visible = false;
            indexAvailabilityQueue.Enqueue(i);
        }
    }

    internal VisualElement RetrieveFromPool(out int index)
    {
        index = -1;
        if(indexAvailabilityQueue.Count > 0)
        {
            index = indexAvailabilityQueue.Dequeue();
            healthBarComponents[index].visible = true;
            return healthBarComponents[index];
        }
        return null;
    }

    internal void ReturnToPool(int index)
    {
        indexAvailabilityQueue.Enqueue(index);
        healthBarComponents[index].visible = false;
    }
}
