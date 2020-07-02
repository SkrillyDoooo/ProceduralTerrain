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
    Camera mainCam;
    Plane terrainPlane;
    BuildData currentBuildData;

    Renderer mouseObjectRenderer;
    int terrainMask;
    int idleClickMask;

    InteractionMode currentZone = InteractionMode.Game;
    GameContext currentGameContext = GameContext.Idle;

    public Game game;

    private void Start()
    {
        game.bottomFocusGained += HandleUIFocus;
        game.middleFocusGained += HandleGameFocus;
        game.topFocusGained += HandleUIFocus;
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
        switch(currentGameContext)
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
        PositionCursorObject();
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000.0f, idleClickMask))
            {
                ISelectableObject selectable = hitInfo.collider.GetComponent<ISelectableObject>();
                if (selectable != null)
                {
                    ClickableObjectData data = selectable.GetClickableObjectData();
                    VisualElement detailRoot = game.SetSelectableContextWindow(data.contextWindowButtons, data.infoPanelTemplate, data.icon, data.name);
                    selectable.SetInfoPanelRoot(detailRoot);
                    game.SetSelectableCommandWindow(data.commandWindowButtons);
                    if (!Input.GetKey(KeyCode.LeftControl))
                        game.ClearSelectables();

                    game.AddSelectable(selectable);
                    selectable.Select();
                }
            }
            else
            {
                game.ClickedNothing();
            }
        }
    }


    void PositionCursorObject()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        float enter = 0.0f;
        if (terrainPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            transform.position = hitPoint;
        }

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000.0f, terrainMask))
        {
            cursorObject.transform.position = hitInfo.point;
        }
    }

    void DoBuildingContext()
    {
        PositionCursorObject();
        if (Input.GetMouseButtonDown(0))
        {
            if(/*isValidPlacement() &&*/ game.TryPurchase(currentBuildData))
            {
                GameObject.Instantiate(currentBuildData.prefab, cursorObject.position, Quaternion.identity);
            }
        }
    }

    void DoTopContext()
    {

    }

    void DoUIContext()
    {

    }
}
