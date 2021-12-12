using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ContextCursor : MonoBehaviour
{


    public enum InteractionMode
    {
        Game,
        UI
    }

    public enum GameContext
    {
        Idle,
        Building
    }

    public Transform cursorObject;
    public Transform navMeshHeightTestObject;

    Camera mainCam;
    Plane terrainPlane;
    BuildData currentBuildData;

    private BoxSelectorController boxSelectorController;

    Renderer mouseObjectRenderer;
    int terrainMask;
    int idleClickMask;

    InteractionMode currentZone = InteractionMode.Game;
    GameContext currentGameContext = GameContext.Idle;

    public TerrainGenerator terrain;

    private void Start()
    {
        Game.Instance.bottomFocusGained += HandleUIFocus;
        Game.Instance.middleFocusGained += HandleGameFocus;
        Game.Instance.topFocusGained += HandleUIFocus;
        idleClickMask = LayerMask.GetMask("Clickable");

        terrainPlane = new Plane(Vector3.up, Vector3.zero);
        mainCam = Camera.main;
        mouseObjectRenderer = cursorObject.GetComponent<Renderer>();
        terrainMask = LayerMask.GetMask("Terrain");
    }

    private void HandleGameFocus()
    {
        currentZone = InteractionMode.Game;
        mouseObjectRenderer.enabled = true;
    }

    private void HandleUIFocus()
    {
        currentZone = InteractionMode.UI;
        mouseObjectRenderer.enabled = false;
    }

    public void Cancel()
    {
        switch(currentGameContext)
        {
            case GameContext.Building:
                DisableBuildContext();
                break;
        }
    }

    public void DisableBuildContext()
    {
        currentGameContext = GameContext.Idle;
    }

    public void EnableBuildContext(BuildData buildObjectData)
    {
        currentGameContext = GameContext.Building;
        currentBuildData = buildObjectData;
    }

    private void Update()
    {
        switch(currentZone)
        {
            case InteractionMode.UI:
                DoUIContext();
                break;
            case InteractionMode.Game:
                DoGameContext();
                break;
        }
    }


    void DoGameContext()
    {
        if (TryGetCursorRaycastPoint(out var point))
        {
            cursorObject.transform.position = point;
            point.y = 0;
            navMeshHeightTestObject.position = new Vector3(point.x, TerrainGenerator.Instance.GetHeightAtCoord(new Vector2(point.x, point.z)) + 20.0f, point.z);
        }
        switch (currentGameContext)
        {
            case GameContext.Idle:
                DoIdleGameContext();
                break;
            case GameContext.Building:
                DoBuildingContext();
                break;
        }
    }

    void DoIdleGameContext()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 100000.0f, idleClickMask))
            {
                ISelectableObject selectable = hitInfo.collider.GetComponent<ISelectableObject>();
                if (selectable != null)
                {
                    ContextPanelData data = selectable.GetContextPanelData();
                    VisualElement detailRoot = Game.Instance.SetSelectableContextWindow(data.contextWindowButtons, data.infoPanelTemplate, data.icon, data.name);
                    if (!Input.GetKey(KeyCode.LeftControl))
                        Game.Instance.ClearSelectables();

                    Game.Instance.SetSelectableCommandWindow(data.commandWindowButtons);
                    Game.Instance.AddSelectable(selectable);
                    selectable.Select(detailRoot);
                }
            }
            else
            {
                Game.Instance.Cancel();
            }
        }

        if(Input.GetMouseButtonDown(1))
        {
            if(TryGetCursorRaycastPoint(out var point))
            {
                Game.Instance.OnRightClick(point);
            }
        }
    }

    public bool TryGetCursorRaycastPoint(out Vector3 point)
    {
        point = Vector3.zero;
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;
        if (terrainPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            transform.position = hitPoint;
        }
        else
        {
            return false;
        }

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 10000000, terrainMask))
        {
            point = hitInfo.point;
            return true;
        }
        else
        {
            return false;
        }
    }

    void DoBuildingContext()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if(/*isValidPlacement() &&*/ Game.Instance.TryPurchase(currentBuildData.cost) && TryGetCursorRaycastPoint(out var point))
            {
                var go = GameObject.Instantiate(currentBuildData.prefab, point, Quaternion.identity);
                PlayerManifest.Instance.AddBuilding(go.transform);
            }
        }


        if (Input.GetMouseButtonDown(1))
        {
            if (TryGetCursorRaycastPoint(out var point))
            {
                Game.Instance.Cancel();
            }
        }
    }

    void DoTopContext()
    {

    }

    void DoUIContext()
    {

    }

    internal void InitialzieBoxSelector(BoxSelectorUI boxSelectorUI)
    {
        boxSelectorController = new BoxSelectorController(boxSelectorUI);
        boxSelectorController.OnReceiveBounds += OnReceiveBounds;
    }

    void OnReceiveBounds(Bounds b)
    {
        List<Transform> units = PlayerManifest.Instance.GetUnitManifest();
        bool unitSelected = false;
        for(int i = 0; i < units.Count; i++)
        {
            Transform t = units[i];
            if(b.Contains(mainCam.WorldToViewportPoint(t.position)))
            {
                ISelectableObject selectable = t.GetComponent<ISelectableObject>();
                if (selectable == null)
                    continue;

                ContextPanelData data = selectable.GetContextPanelData();
                VisualElement detailRoot = Game.Instance.SetSelectableContextWindow(data.contextWindowButtons, data.infoPanelTemplate, data.icon, data.name);
                Game.Instance.SetSelectableCommandWindow(data.commandWindowButtons);

                unitSelected = true;
                selectable.Select(detailRoot);
                Game.Instance.AddSelectable(selectable);
            }
        }

        if(!unitSelected)
        {
            List<Transform> buildiings = PlayerManifest.Instance.GetBuildingManifest();
            for (int i = 0; i < buildiings.Count; i++)
            {
                Transform t = buildiings[i];
                if (b.Contains(mainCam.WorldToViewportPoint(t.position)))
                {
                    ISelectableObject selectable = t.GetComponent<ISelectableObject>();
                    if (selectable == null)
                        continue;

                    ContextPanelData data = selectable.GetContextPanelData();
                    VisualElement detailRoot = Game.Instance.SetSelectableContextWindow(data.contextWindowButtons, data.infoPanelTemplate, data.icon, data.name);
                    Game.Instance.SetSelectableCommandWindow(data.commandWindowButtons);

                    Game.Instance.AddSelectable(selectable);
                    selectable.Select(detailRoot);
                }
            }
        }
    }
}
