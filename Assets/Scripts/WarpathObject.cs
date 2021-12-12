using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class WarpathObject : MonoBehaviour, ISelectableObject
{
    public WarpathObjectData data;
    public event EventHandler RemoveFromSelectionList;
    SelectionComponent selection;
    HealthBarUI healthUI;
    protected bool selected;

    public int currentHealth { get; private set; }
    public void UpdateHealth(int healthToAdd)
    {
        if (currentHealth <= 0)
        {
            currentHealth += healthToAdd;
            healthUI.UpdateWidthPercent(0);
            DestroyWarpathObject();
        }
        else
        {
            healthUI.UpdateWidthPercent((float)currentHealth / (float)data.maxHealth);
        }

    }

    protected abstract void DestroyWarpathObject();

    public virtual void CommandButtonPressed(int buttonId)
    {
        throw new NotImplementedException();
    }

    public virtual void ContextButtonPressed(int index)
    {
        throw new NotImplementedException();
    }

    public virtual void Deselect()
    {
        healthUI.Disable();
        selection.SetProjectorActive(false);
        selected = false;
    }

    public virtual ContextPanelData GetContextPanelData()
    {
        throw new NotImplementedException();
    }

    public virtual void RightClick(Vector3 rightClickPoint)
    {
        throw new NotImplementedException();
    }

    public void RemoveObjectFromSelectionList()
    {
        RemoveFromSelectionList.Invoke(this, new EventArgs());
    }

    protected virtual void Start()
    {
        currentHealth = data.maxHealth;
        healthUI = GetComponent<HealthBarUI>();
        healthUI.UpdateWidthPercent((float)currentHealth / (float)data.maxHealth);
        selection = GetComponentInChildren<SelectionComponent>();
        selection.SetProjectorActive(false);
        if (selection == null)
            Debug.LogError("Warpath object lacking a slection component. Please add the selection prefab as a child of this object's prefab");
    }

    public virtual void Select(VisualElement root)
    {
        healthUI.Enable();
        selection.SetProjectorActive(true);
        selected = true;
    }
}
