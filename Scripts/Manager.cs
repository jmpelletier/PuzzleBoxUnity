using PuzzleBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Manager : PuzzleBox.Manager
{
    [Header("Camera")]
    public Camera mainCamera;

    [Header("Scenes")]
    public string gameScene = "";

    [Header("UI")]
    public PuzzleBox.UIScreen startScreen;
    public PuzzleBox.UIScreen quitConfirmScreen;
    public PuzzleBox.UIScreen pauseScreen;
    public PuzzleBox.UIScreen endScreen;


    // Start is called before the first frame update
    void Start()
    {
        quitConfirmScreen?.Hide(false);
        pauseScreen?.Hide(false);
        endScreen?.Hide(false);

        startScreen?.Show(false);

        if (mainCamera != null )
        {
            mainCamera.gameObject.SetActive(true);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void ConfirmQuitGame()
    {
        quitConfirmScreen?.Show();
    }

    public override void CancelQuitGame()
    {
        quitConfirmScreen?.Hide();

        if (pauseScreen != null && pauseScreen.isActiveAndEnabled)
        {
            pauseScreen.interactable = true;
        }

        if (endScreen != null && endScreen.isActiveAndEnabled)
        {
            endScreen.interactable = true;
        }

        if (startScreen != null && startScreen.isActiveAndEnabled)
        {
            startScreen.interactable = true;
        }
    }

    public override void ClearGame()
    {
        quitConfirmScreen?.Hide(false);
        pauseScreen?.Hide(false);
        startScreen?.Hide(false);

        endScreen?.Show(true);

        mainCamera.gameObject.SetActive(true);
    }

    public override void EndPlay()
    {
        base.EndPlay();

        LevelManager.UnloadSubScene();

        pauseScreen?.Hide(true);
        startScreen.Show(true);
    }

    public override void StartPlay()
    {
        base.StartPlay();

#if UNITY_EDITOR
        if(!EditorUtilities.SceneIsIncludedInBuild(gameScene))
        {
            Debug.LogError($"ゲームは開始できない。シーンがビルド設定に追加されていない。 : {gameScene}");
            return;
        }
#endif
        LevelManager.LoadLevelAdditive(gameScene);

        mainCamera.gameObject.SetActive(false);

        quitConfirmScreen?.Hide(false);
        pauseScreen?.Hide(false);
        endScreen?.Hide(false);
        startScreen?.Hide(true);
    }

    public override void Pause()
    {
        base.Pause();

        pauseScreen?.Show();
        foreach(LevelManager level in LevelManager.instances)
        {
            level.Pause();
        }
    }

    public override void Unpause()
    {
        base.Unpause();

        pauseScreen?.Hide();
        foreach (LevelManager level in LevelManager.instances)
        {
            level.Resume();
        }
    }

    public override void ShowTitle()
    {
        quitConfirmScreen?.Hide(false);
        pauseScreen?.Hide(false);

        endScreen?.Hide(true);
        startScreen?.Show(true);
    }
}
