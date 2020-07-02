using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public interface ISelectableObject
{
    event EventHandler Remove;
    ClickableObjectData GetClickableObjectData();
    void SetInfoPanelRoot(VisualElement root);
    void Deselect();
    void ContextButtonPressed(int index);
    void CommandButtonPressed(int buttonId);
    void Select();
}

[System.Serializable]
public struct ClickableObjectData
{
    public WindowButtonsData contextWindowButtons;
    public VisualTreeAsset infoPanelTemplate;
    public Texture2D icon;
    public string name;
    public WindowButtonsData commandWindowButtons;
}
