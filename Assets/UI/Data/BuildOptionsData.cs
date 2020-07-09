using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Warpath UI Data/Building Options")]
public class BuildOptionsData : ScriptableObject
{
    public WindowButtonsData buildWindowData;
    public VisualTreeAsset treeAsset;
    
    public BuildData[] buildData;

    private void OnValidate()
    {
        if (buildWindowData != null && buildData.Length != buildWindowData.buttons.Length)
        {
            BuildData[] tmp = new BuildData[buildWindowData.buttons.Length];
            Array.Copy(buildData, tmp, buildData.Length > (int)buildWindowData.buttons.Length ? (int)buildWindowData.buttons.Length : buildData.Length);
            buildData = tmp;
            for(int i = 0; i < buildWindowData.buttons.Length; i++)
            {
                buildData[i].name = buildWindowData.buttons[i].name;
            }
        }
        else if(buildWindowData == null)
        {
            buildData = new BuildData[0];
            Debug.LogError("Please set the corresponding button data for the build window");
        }
        else
        {
            for (int i = 0; i < buildWindowData.buttons.Length; i++)
            {
                buildData[i].name = buildWindowData.buttons[i].name;
            }
        }
    }
}


[System.Serializable]
public struct BuildData
{
    [ReadOnlyEditor] public string name; 
    public GameObject prefab;
    public ResourceManager.ItemCost[] cost;

    private Dictionary<ResourceManager.ResourceType, int> itemLedger;


    private void InitItemLedger()
    {
        itemLedger = new Dictionary<ResourceManager.ResourceType, int>();

        for (int i = 0; i < cost.Length; i++)
        {
            itemLedger.Add(cost[i].type, cost[i].cost);
        }
    }


    public int GetResouceCost(ResourceManager.ResourceType type)
    {
        if (cost == null)
            return 0;
        if (itemLedger == null)
            InitItemLedger();

        if(itemLedger.TryGetValue(type, out var value))
        {
            return value;
        }
        return 0;
    }
    //cost
    //etc.
}
