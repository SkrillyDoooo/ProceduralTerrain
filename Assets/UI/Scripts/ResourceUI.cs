using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceUI
{
    VisualElement resourceRoot;
    Dictionary<ResourceManager.ResourceType, Label> resourceLabels = new Dictionary<ResourceManager.ResourceType, Label>();
    public ResourceUI(VisualElement root)
    {
        resourceRoot = root.Query<VisualElement>(name: "resources");

        resourceLabels.Add(ResourceManager.ResourceType.Gold, GetLabel("gold"));
        resourceLabels.Add(ResourceManager.ResourceType.Glory, GetLabel("glory"));
    }

    public void UpdateResource(ResourceManager.ResourceType type, int amount)
    {
        resourceLabels[type].text = amount.ToString();
    }

    private Label GetLabel(string name)
    {
        VisualElement root = resourceRoot.Query<VisualElement>(name: name);
        return root.Query<Label>("amount");
    }

    public void UpdateAllResources(Dictionary<ResourceManager.ResourceType, int> values)
    {
        foreach(KeyValuePair<ResourceManager.ResourceType, int> keyValuePair in values)
        {
            UpdateResource(keyValuePair.Key, keyValuePair.Value);
        }
    }
}
