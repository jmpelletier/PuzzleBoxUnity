using PuzzleBox;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class LevelManager : PuzzleBox.LevelManager
{
    private PlatformerActions inputActions;

    protected override void Awake()
    {
        base.Awake();

        inputActions = new PlatformerActions();
        inputActions.UI.Pause.performed += (context) => {
            OnPause();
        };
        inputActions.UI.Enable();
    }

    protected virtual void OnEnable()
    {
        inputActions.UI.Enable();
    }

    protected virtual void OnDisable()
    {
        inputActions.UI.Disable();
    }
}
