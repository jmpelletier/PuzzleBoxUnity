/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    public class ClearGame : ActionDelegate
    {
        public override void Perform(GameObject sender)
        {
            PerformAction(() => {
                if (Manager.instance != null)
                {
                    LevelManager.UnloadSubScene();
                    Manager.instance.ClearGame();
                }
                else
                {
                    Debug.Log("Game Clear!");
                    LevelManager.ClearSaveState();
                    SceneTransition.ReloadCurrentScene();
                }
            });
        }
    }
}

