using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public interface ISelectableObject
{
    event EventHandler RemoveFromSelectionList;
    ContextPanelData GetContextPanelData();
    void Deselect();
    void ContextButtonPressed(int index);
    void CommandButtonPressed(int buttonId);
    void Select(VisualElement root);
    void RightClick(Vector3 rightClickPoint);
}

[System.Serializable]
public struct ContextPanelData
{
    public WindowButtonsData contextWindowButtons;
    public VisualTreeAsset infoPanelTemplate;
    public Texture2D icon;
    public string name;
    public WindowButtonsData commandWindowButtons;
}
