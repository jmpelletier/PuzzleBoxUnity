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
            LevelManager.LoadLevel(sceneName, spawnPointUID);
        }


        GUIStyle guiStyle = new GUIStyle();

        private void OnDrawGizmos()
        {
            bool sceneIncluded = EditorUtilities.SceneIsIncludedInBuild(sceneName);

            Color color = sceneIncluded ? Color.blue : Color.red;
            Gizmos.color = color;
            Gizmos.DrawLine(transform.position + Vector3.left * 0.5f, transform.position);
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(-0.25f, 0.25f));
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(-0.25f, -0.25f));

            guiStyle.normal.textColor = color;
            guiStyle.alignment = TextAnchor.MiddleLeft;

            string labelText = sceneIncluded ? "���x���ړ��F" : "���x���ړ��iBuild���o�^�I�j�F";
            labelText += sceneName;
            if (spawnPointUID != "")
            {
                labelText += $" ({spawnPointUID})";
            }

            Handles.Label(transform.position + Vector3.right * 0.1f, labelText, guiStyle);
        }
    }
}

