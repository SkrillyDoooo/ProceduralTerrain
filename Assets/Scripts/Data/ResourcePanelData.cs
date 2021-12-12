using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Warpath UI Data/Resource Panel")]
public class ResourcePanelData : ScriptableObject
{
    public ResourceUIData[] data;
    private Dictionary<ResourceManager.ResourceType, Texture2D> dataLookup;

    private void BuildDictionary()
    {
        dataLookup = new Dictionary<ResourceManager.ResourceType, Texture2D>();
        for(int i = 0; i < data.Length; i++)
        {
            dataLookup.Add(data[i].type, data[i].icon);
        }
    }
    public Texture2D GetResourceIcon(ResourceManager.ResourceType type)
    {
        if (dataLookup == null)
            BuildDictionary();
        return dataLookup[type];
    }
}

[System.Serializable]
public struct ResourceUIData
{
    public ResourceManager.ResourceType type;
    public Texture2D icon;
}
