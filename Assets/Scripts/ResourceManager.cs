using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{

    public enum ResourceType
    {
        Gold, 
        Glory
    }


    [System.Serializable]
    public struct ItemCost
    {
        public ResourceManager.ResourceType type;
        public int cost;
    }


    Dictionary<ResourceType, int> ledger = new Dictionary<ResourceType, int>();

    public void InitLedger()
    {
        ledger.Add(ResourceType.Gold, 100);
        ledger.Add(ResourceType.Glory, 100);
    }

    public Dictionary<ResourceType, int> GetLedger()
    {
        return ledger;
    }

    public bool CanAfford(ResourceType type, int amount)
    {
        return ledger[type] - amount >= 0;
    }

    public void Spend(ResourceType type, int amount)
    {
        if(CanAfford(type, amount))
        {
            ledger[type] -= amount;
        }
    }

    public void Add(ResourceType type, int amount)
    {
        ledger[type] += amount;
    }

    public int GetValue(ResourceType type)
    {
        return ledger[type];
    }

    // Update is called once per frame
    void Update()
    {
        // run resource generators 
    }
}
