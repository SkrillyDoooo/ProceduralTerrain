using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using System.Security.Principal;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ProductionBuildingUI : MonoBehaviour, ISelectableObject
{

    [SerializeField]
    private ClickableObjectData clickableData;

    VisualElement infoPanelUI;
    List<Button> productionQueueButtons;
    VisualElement progressBar;
    VisualElement progressBarBackground;
    const int maxProduction = 5;
    ProductionBuilding productionBuilding;
    SelectionComponent selection;
    Length progressBarLength;
    bool selected = false;
    public event EventHandler Remove;

    void Start()
    {
        productionBuilding = GetComponent<ProductionBuilding>();
        productionBuilding.UnitComplete += UnitComplete;
        selection = GetComponentInChildren<SelectionComponent>();
        selection.SetProjectorActive(false);
    }

    void Update()
    {
        if(selected && progressBar != null)
        {
            UpdateProgress();
        }
    }

    private void UnitComplete()
    {
        UpdateProductionButtons();
    }

    enum CommandButtonIds
    {
        Destroy = 6
    }

    void CancelItemCallback(ClickEvent evt)
    {
        {
            if (evt.target is Button button)
            {
                int index = productionQueueButtons.IndexOf(button);
                if (index < productionBuilding.productionQueueCount)
                {
                    RemoveItem(index);
                }
            }
        }
    }

    private void AddItem(int optionIndex)
    {
        if (productionBuilding.productionQueueCount >= maxProduction)
            return;

        if (!productionBuilding.TryAddItem(optionIndex))
            return;
        UpdateProductionButtons();
    }

    private void RemoveItem(int index)
    {
        if (productionBuilding.productionQueueCount == 0)
            return;

        productionBuilding.RemoveItem(index);
        UpdateProductionButtons();
    }

    public ClickableObjectData GetClickableObjectData()
    {
        return clickableData;
    }

    // remove and set up base class for production building Info panel that can handle multi-selection of buildings
    public void SetInfoPanelRoot(VisualElement root)
    {
        infoPanelUI = root;
        productionQueueButtons = infoPanelUI.Query<Button>(name: "button").ToList();
        progressBarLength = new Length(0, LengthUnit.Percent);
        progressBar = infoPanelUI.Query<VisualElement>(className: "progress");

        infoPanelUI.RegisterCallback<ClickEvent>(CancelItemCallback);

        UpdateProgress();
        UpdateProductionButtons();
    }

    private void UpdateProgress()
    {
        progressBarLength.value = productionBuilding.currentProgress * 100;
        progressBar.style.width = progressBarLength;
    }

    private void UpdateProductionButtons()
    {
        Unit[] queue = productionBuilding.GetCurrentProductionArray();
        int i = 0;
        for(i = 0; i < queue.Length; i++)
        {
            productionQueueButtons[i].style.backgroundImage = queue[i].icon;
        }

        for (int j = i;  j < maxProduction; j++)
        {
            productionQueueButtons[j].style.backgroundImage = null;
        }
    }

    public void Deselect()
    {
        infoPanelUI.UnregisterCallback<ClickEvent>(CancelItemCallback);
        selected = false;
        selection.SetProjectorActive(false);
    }

    public void ContextButtonPressed(int index)
    {
        AddItem(index);
    }

    public void CommandButtonPressed(int buttonId)
    {
        switch(buttonId)
        {
            case ((int)CommandButtonIds.Destroy):
                Demolish();
                break;
        }
    }

    public void Demolish()
    {
        Remove.Invoke(this, new EventArgs());
        selection.SetProjectorActive(false);

        productionBuilding.Demolish();
        Destroy(productionBuilding);
        Destroy(gameObject, 0.2f);
    }

    public void Select()
    {
        selected = true;
        selection.SetProjectorActive(true);
    }
}
