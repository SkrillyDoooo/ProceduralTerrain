using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceUI
{
    VisualElement resourceRoot;
    Dictionary<ResourceManager.ResourceType, Label> resourceLabels = new Dictionary<ResourceManager.ResourceType, Label>();
    ResourcePanelData m_Data;
    public ResourceUI(VisualElement root, ResourcePanelData uiData)
    {
        resourceRoot = root.Query<VisualElement>(name: "resources");

        resourceLabels.Add(ResourceManager.ResourceType.Gold, GetLabel("gold"));
        resourceLabels.Add(ResourceManager.ResourceType.Glory, GetLabel("glory"));

        m_Data = uiData;
        SetIcon(ResourceManager.ResourceType.Gold, "gold");
        SetIcon(ResourceManager.ResourceType.Glory, "glory");
    }

    public void UpdateResource(ResourceManager.ResourceType type, int amount)
    {
        resourceLabels[type].text = amount.ToString();
    }

    private void SetIcon(ResourceManager.ResourceType type, string name)
    {
        VisualElement root = resourceRoot.Query<VisualElement>(name: name);
       
        VisualElement image = root.Query<VisualElement>("image");
        image.style.backgroundImage = m_Data.GetResourceIcon(type);
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
