/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PuzzleBox
{
    public class GotoLevel : ActionDelegate
    {
        public string sceneName = "";
        public string spawnPointUID = "";

        // Start is called before the first frame update
        void Start()
        {

        }

        public override void Perform(GameObject sender)
        {
            PerformAction(() => LevelManager.LoadLevel(sceneName, spawnPointUID));
        }
    }
}

