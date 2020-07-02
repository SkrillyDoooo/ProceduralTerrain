﻿using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using System;

[RequireComponent(typeof(UIDocument), typeof(EventSystem))]
public class Game: MonoBehaviour
{
    private const string ActiveClassName = "game-button--active";

    [SerializeField] private PanelSettings panelSettings = default;
    [SerializeField] private VisualTreeAsset sourceAsset = default;

    public event System.Action bottomFocusGained;
    public event System.Action middleFocusGained;
    public event System.Action topFocusGained;

    ContextWindowUI contextWindow;
    CommandWindowUI commandWindow;
    VisualElement vsBracket;


    int maxSelectableObjects = 2048;
    List<ISelectableObject> currentSelectableObjects;


    [EnumNamedArrayAttribute(typeof(CommandWindowUI.CommandWindowMode))]
    public WindowButtonsData[] commandWindowButtonsData = new WindowButtonsData[(int)CommandWindowUI.CommandWindowMode.Count];
    [EnumNamedArrayAttribute(typeof(ContextWindowUI.ContextWindowMode))]
    public WindowButtonsData[] contextWindowButtonsData = new WindowButtonsData[(int)ContextWindowUI.ContextWindowMode.Count];
    public BuildOptionsData buildOptionsData;
    public ContextCursor contextInteractor;
    ResourceManager resourceManager;
    ResourceUI resourceUI;

    public void SetPanelSettings(PanelSettings newPanelSettings)
    {
        panelSettings = newPanelSettings;
        var uiDocument = GetComponent<UIDocument>();
        uiDocument.panelSettings = panelSettings;
    }

    private void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        uiDocument.panelSettings = panelSettings;
        uiDocument.visualTreeAsset = sourceAsset;
        currentSelectableObjects = new List<ISelectableObject>(maxSelectableObjects);
        InitializeVisualTree(uiDocument);
        resourceManager = GetComponent<ResourceManager>();
        resourceManager.InitLedger();
        resourceUI.UpdateAllResources(resourceManager.GetLedger());
    }

    private void InitializeVisualTree(UIDocument doc)
    {
        var root = doc.rootVisualElement;

        var bottom = root.Q(name: "bottom");
        var middle = root.Q(name: "middle");
        var top = root.Q(name: "top");
        this.vsBracket = root.Query<VisualElement>(name: "brackets");
        this.vsBracket.visible = false;

        root.RegisterCallback<MouseOutEvent>(evt =>
        {
            if (evt.target is Button button)
            {
                vsBracket.visible = false;
                button.Remove(vsBracket);
            }
        });

        root.RegisterCallback<MouseOverEvent>(evt =>
        {
            if (evt.target is Button button)
            {
                button.Add(vsBracket);
                vsBracket.visible = true;
            }
        });

        var commandWindowRoot = bottom.Q(name: "command-window");
        var contextWindowRoot = bottom.Q(name: "context-window");

        commandWindow = new CommandWindowUI(commandWindowRoot, commandWindowButtonsData);
        commandWindow.BuildButtonPressed += OpenBuildOptions;
        commandWindow.CancelButtonPressed += Cancel;
        commandWindow.SelectableButtonPressed += CommandSelectableButtonPressed;

        contextWindow = new ContextWindowUI(contextWindowRoot, contextWindowButtonsData);
        contextWindow.BuildButtonPressed += StartBuildMode;
        contextWindow.SelectableButtonPressed += ContextSelectableButtonPressed;

        resourceUI = new ResourceUI(top);

        top.RegisterCallback<MouseEnterEvent>(evt => {
            topFocusGained();
        });

        middle.RegisterCallback<MouseEnterEvent>(evt => {
            middleFocusGained();
        });

        bottom.RegisterCallback<MouseEnterEvent>(evt => {
            bottomFocusGained();
        });
    }

    private void CommandSelectableButtonPressed(int buttonId)
    {
        // these objects might get deleted so we should copy the buttons to iterate over them.
        ISelectableObject[] selectables = currentSelectableObjects.ToArray();
        for(int i = 0; i < selectables.Length; i++)
        {
            selectables[i].CommandButtonPressed(buttonId);
        }
    }

    public bool TryPurchase(BuildData currentBuildData)
    {
        for(int i = 0; i < currentBuildData.cost.Length; i ++)
        {
            if (!resourceManager.CanAfford(currentBuildData.cost[i].type, currentBuildData.cost[i].cost))
                return false;
        }

        for (int i = 0; i < currentBuildData.cost.Length; i++)
        {
            ResourceManager.ResourceType type = currentBuildData.cost[i].type;
            resourceManager.Spend(type, currentBuildData.cost[i].cost);
            resourceUI.UpdateResource(type, resourceManager.GetValue(type));
        }

        return true;
    }

    private void ContextSelectableButtonPressed(int index)
    {
        for (int i = 0; i < currentSelectableObjects.Count; i++)
        {
            currentSelectableObjects[i].ContextButtonPressed(index);
        }
    }

    internal void AddSelectable(ISelectableObject selectable)
    {
        currentSelectableObjects.Add(selectable);
        selectable.Remove += Remove;
    }

    private void Remove(object sender, EventArgs e)
    {
        currentSelectableObjects.Remove((ISelectableObject)sender);
        if(currentSelectableObjects.Count == 0)
        {
            contextWindow.SetContext(ContextWindowUI.ContextWindowMode.Idle);
            commandWindow.UpdateMode(CommandWindowUI.CommandWindowMode.Idle);
        }
    }

    internal void ClearSelectables()
    {
        for (int i = 0; i < currentSelectableObjects.Count; i++)
        {
            currentSelectableObjects[i].Deselect();
        }
        currentSelectableObjects.Clear();
    }

    public void SetCommandWindowMode(CommandWindowUI.CommandWindowMode mode)
    {
        commandWindow.UpdateMode(mode);
    }

    public VisualElement SetSelectableContextWindow(WindowButtonsData windowData,  VisualTreeAsset treeAsset, Texture2D texture, string name)
    {
        contextWindow.SetSelectableContextWindow(windowData);
        return contextWindow.SetInfoPanel(texture, name, treeAsset);
    }

    public void SetSelectableCommandWindow(WindowButtonsData windowData)
    {
        commandWindow.SetSelectableCommandWindow(windowData);
    }

    public void ClickedNothing()
    {
        contextWindow.SetContext(ContextWindowUI.ContextWindowMode.Idle);
        ClearSelectables();
    }

    private void StartBuildMode(int index)
    {
        BuildOptionsData buildOptions = buildOptionsData;
        BuildData data = buildOptions.buildData[index];
        VisualElement buildInfo = contextWindow.SetInfoPanel(contextWindowButtonsData[(int)ContextWindowUI.ContextWindowMode.Build].buttons[index].icon, data.name, buildOptions.treeAsset);
        VisualElement gold = GetCostPanel(buildInfo, "gold", out var goldLabel);
        VisualElement glory = GetCostPanel(buildInfo, "glory", out var gloryLabel);

        int goldCost = data.GetResouceCost(ResourceManager.ResourceType.Gold);
        if (goldCost > 0)
        {
            gold.style.display = DisplayStyle.Flex;
            goldLabel.text = goldCost.ToString();
        }
        else
        {
            gold.style.display = DisplayStyle.None;
        }

        int gloryCost = data.GetResouceCost(ResourceManager.ResourceType.Glory);

        if (gloryCost > 0)
        {
            glory.style.display = DisplayStyle.Flex;
            gloryLabel.text = gloryCost.ToString();
        }
        else
        {
            glory.style.display = DisplayStyle.None;
        }

        contextInteractor.EnableBuildContext(data);
    }

    VisualElement GetCostPanel(VisualElement root, string name, out Label amount)
    {
        VisualElement cost = root.Query<VisualElement>(name: name);
        amount = cost.Query<Label>(name: "amount");
        return cost;
    }

    public void Cancel()
    {
        contextInteractor.Cancel();
        contextWindow.SetContext(ContextWindowUI.ContextWindowMode.Idle);
        commandWindow.UpdateMode(CommandWindowUI.CommandWindowMode.Idle);
        ClearSelectables();
    }

    public void Build()
    {
        contextWindow.SetContext(ContextWindowUI.ContextWindowMode.Build);
        commandWindow.UpdateMode(CommandWindowUI.CommandWindowMode.Build);
    }

    private void OpenBuildOptions()
    {
        contextWindow.SetContext(ContextWindowUI.ContextWindowMode.Build);
    }

    private void Update()
    {
        commandWindow.DoCommandWindow();
        contextWindow.DoContextWindow();

        vsBracket.style.height = 48 + Mathf.Sin(Time.time * 10) * 5;
        vsBracket.style.width = 48 + Mathf.Sin(Time.time * 10) * 5;
    }

    private void OnValidate()
    {
        if(commandWindowButtonsData.Length != (int)CommandWindowUI.CommandWindowMode.Count)
        {
            WindowButtonsData[] tmp = new WindowButtonsData[(int)CommandWindowUI.CommandWindowMode.Count];
            Array.Copy(commandWindowButtonsData, tmp, commandWindowButtonsData.Length > (int)CommandWindowUI.CommandWindowMode.Count ? (int)CommandWindowUI.CommandWindowMode.Count : commandWindowButtonsData.Length);
            commandWindowButtonsData = tmp;
        }

        if(contextWindowButtonsData.Length != (int)ContextWindowUI.ContextWindowMode.Count)
        {
            WindowButtonsData[] tmp = new WindowButtonsData[(int)ContextWindowUI.ContextWindowMode.Count];
            Array.Copy(contextWindowButtonsData, tmp, contextWindowButtonsData.Length > (int)ContextWindowUI.ContextWindowMode.Count ? (int)ContextWindowUI.ContextWindowMode.Count : contextWindowButtonsData.Length);
            contextWindowButtonsData = tmp;
        }
    }
}
