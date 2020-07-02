using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIHotkeyControls : MonoBehaviour
{

    public KeyCode cancel = KeyCode.Escape;
    public KeyCode build = KeyCode.B;

    public Game game;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(cancel))
        {
            game.Cancel();
        }

        if(Input.GetKeyDown(build))
        {
            game.Build();
        }
    }
}
