using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ProductionBuildingUI : MonoBehaviour
{

    [SerializeField]
    private ContextPanelData contextPanelData;

    VisualElement infoPanelUI;
    List<Button> productionQueueButtons;
    VisualElement progressBar;
    VisualElement progressBarBackground;
    const int maxProduction = 5;
    Length progressBarLength;

    public event System.Action<int> CancelItem;

    public enum CommandButtonIds
    {
        Destroy = 6
    }

    void CancelItemCallback(ClickEvent evt)
    {
        if (evt.target is Button button)
        {
            int index = productionQueueButtons.IndexOf(button);
            CancelItem(index);
        }
    }

    public ContextPanelData GetContextPanelData()
    {
        return contextPanelData;
    }

    public void UpdateProgress(float currentProgress)
    {
        if(progressBar != null)
        {
            progressBarLength.value = currentProgress * 100;
            progressBar.style.width = progressBarLength;
        }
    }

    public void UpdateProductionButtons(UnitInfo[] queue)
    {
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
    }

    public void Select(VisualElement root, UnitInfo[] queue, float currentProgress)
    {
        infoPanelUI = root;
        productionQueueButtons = infoPanelUI.Query<Button>(name: "button").ToList();
        progressBarLength = new Length(0, LengthUnit.Percent);
        progressBar = infoPanelUI.Query<VisualElement>(className: "progress");

        infoPanelUI.RegisterCallback<ClickEvent>(CancelItemCallback);

        UpdateProgress(currentProgress);
        UpdateProductionButtons(queue);
    }
}
