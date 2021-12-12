using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

public class CommandWindowUI
{
    public enum CommandWindowMode
    {
        Idle,
        Build,
        Selectable,
        Count
    }

    CommandWindowMode mode;
    private List<Button> buttons;
    VisualElement root;
    WindowButtonsData[] buttonsData;

    EventCallback<ClickEvent> currentCallback;

    public event System.Action BuildButtonPressed;
    public event System.Action CancelButtonPressed;
    public event System.Action<int> SelectableButtonPressed;

    public CommandWindowUI(VisualElement root, WindowButtonsData[] windowButtonsData)
    {
        this.root = root;
        this.buttons = root.Query<Button>(name: "button").ToList();
        this.buttonsData = windowButtonsData;

        mode = CommandWindowMode.Idle;
        currentCallback = IdleCallback;
        UpdateMode(mode);
    }

    public void DoCommandWindow()
    {

    }

    public void UpdateMode(CommandWindowMode mode)
    {
        this.mode = mode;
        root.UnregisterCallback<ClickEvent>(currentCallback);
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].visible = false;
        }

        switch (mode)
        {
            case CommandWindowMode.Idle:
                buttons[0].visible = true;
                buttons[0].style.backgroundImage = buttonsData[(int)mode].buttons[0].icon;
                RegisterCallback(IdleCallback);
                break;
            case CommandWindowMode.Build:
                buttons[0].visible = true;
                buttons[0].style.backgroundImage = buttonsData[(int)mode].buttons[0].icon;
                RegisterCallback(CancelCallback);
                break;
            case CommandWindowMode.Selectable:
                RegisterCallback(SelectableCallback);
                break;
        }
    }

    public void SetSelectableCommandWindow(WindowButtonsData buttonsData)
    {
        if (buttonsData == null)
            return;
        UpdateMode(CommandWindowMode.Selectable);
        for (int i = 0; i < buttonsData.buttons.Length; i++)
        {
            if(buttonsData.buttons[i].icon != null)
            {
                buttons[i].visible = true;
                buttons[i].style.backgroundImage = buttonsData.buttons[i].icon;
            }
        }
    }

   void RegisterCallback(EventCallback<ClickEvent> callback)
    {
        currentCallback = callback;
        root.RegisterCallback<ClickEvent>(callback);
    }

    void IdleCallback(ClickEvent evt)
    {
        if (evt.target is Button targetButton)
        {
            if(root.IndexOf(targetButton) == 0)
            {
                BuildButtonPressed();
                UpdateMode(CommandWindowMode.Build);
            }

            evt.StopImmediatePropagation();
        }
    }

    void CancelCallback(ClickEvent evt)
    {
        if (evt.target is Button targetButton)
        {
            if (root.IndexOf(targetButton) == 0)
            {
                CancelButtonPressed();
                UpdateMode(CommandWindowMode.Idle);
            }

            evt.StopImmediatePropagation();
        }
    }

    void SelectableCallback(ClickEvent evt)
    {
        if(evt.target is Button targetButton)
        {
            int buttonId = root.IndexOf(targetButton);
            if (buttonId == 0)
            {
                CancelButtonPressed();
            }
            else
            {
                SelectableButtonPressed(buttonId);
            }
            evt.StopImmediatePropagation();
        }
    }
}
