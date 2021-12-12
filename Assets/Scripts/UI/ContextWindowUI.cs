using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ContextWindowUI
{
    public enum ContextWindowMode
    {
        Idle,
        Build,
        Selectable,
        Count 
    }

    ContextWindowMode mode;
    private List<Button> buttons;
    VisualElement root;

    VisualElement buttonPanel;

    WindowButtonsData[] buttonsData;

    EventCallback<ClickEvent> currentCallback;
    public event System.Action<int> BuildButtonPressed;
    public event System.Action<int> SelectableButtonPressed;
    InfoPanelUI infoPanelUI;

    public ContextWindowUI(VisualElement root, WindowButtonsData[] windowButtonsData)
    {
        this.root = root;
        this.buttonPanel = root.Query<VisualElement>(name: "button-panel");
        this.buttons = buttonPanel.Query<Button>(name: "button").ToList();

        var infoPanel = root.Query<VisualElement>(name: "info-panel");
        infoPanelUI = new InfoPanelUI(infoPanel);
        infoPanelUI.DisableInfoPanel();

        foreach (Button b in buttons)
        {
            b.visible = false;
        }
        this.buttonsData = windowButtonsData;
        mode = ContextWindowMode.Idle;
    }

    public void SetContext(ContextWindowMode mode)
    {
        this.mode = mode;
        switch (mode)
        {
            case ContextWindowMode.Idle:
                infoPanelUI.DisableInfoPanel();
                UnregisterCurrentCallback();
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].visible = false;
                }
                break;
            case ContextWindowMode.Build:
                WindowButtonsData buildWindowButtonData = buttonsData[(int)ContextWindowMode.Build];
                SetButtonsPanel(buildWindowButtonData, BuildCallback);
                break;
        }
    }

    private void SetButtonsPanel(WindowButtonsData windowData, EventCallback<ClickEvent> callback)
    {
        if (windowData == null || callback == null)
            return;
        UnregisterCurrentCallback();
        RegisterCallback(callback);
        for (int i = 0; i < windowData.buttons.Length; i++)
        {
            buttons[i].visible = true;
            buttons[i].style.backgroundImage = windowData.buttons[i].icon;
        }
    }

    public void SetSelectableContextWindow(WindowButtonsData buttons)
    {
        SetButtonsPanel(buttons, SelectableCallback);
        mode = ContextWindowMode.Selectable;
    }

    public VisualElement SetInfoPanel(Texture2D image, string name, VisualTreeAsset detailPane)
    {
        return infoPanelUI.SetInfoPanel(image, name, detailPane);
    }

    void RegisterCallback(EventCallback<ClickEvent> callback)
    {
        currentCallback = callback;
        buttonPanel.RegisterCallback<ClickEvent>(callback);
    }

    void UnregisterCurrentCallback()
    {
        if (currentCallback == null)
            return;
        buttonPanel.UnregisterCallback<ClickEvent>(currentCallback);
    }

    void BuildCallback(ClickEvent evt)
    {
        if (evt.target is Button targetButton)
        {
            BuildButtonPressed(buttons.IndexOf(targetButton));
            evt.StopImmediatePropagation();
        }
    }

    void SelectableCallback(ClickEvent evt)
    {
        if(evt.target is Button targetButton)
        {
            SelectableButtonPressed(buttons.IndexOf(targetButton));
            evt.StopImmediatePropagation();
        }
    }

    public void DoContextWindow()
    {

    }
}
