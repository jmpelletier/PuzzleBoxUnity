/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using PuzzleBox;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PuzzleBox
{
    [RequireComponent(typeof(UniqueID))]
    public class SetSpawnPoint : ActionDelegate
    {
        public enum Persistence
        {
            Reload,
            Session,
            Save
        }

        public Persistence persistence = Persistence.Reload;

        // Start is called before the first frame update
        void Start()
        {

        }

        public override void Perform(GameObject sender)
        {
            string json = JsonUtility.ToJson(transform.position);
            switch (persistence)
            {
                case Persistence.Reload:
                    LevelManager.saveState["PlayerSpawnPosition"] = json;
                    break;
                case Persistence.Session:
                    LevelManager.saveState["PlayerSpawnPosition"] = json;
                    Manager.saveState["PlayerSpawnPosition"] = json;
                    break;
                case Persistence.Save:
                    LevelManager.saveState["PlayerSpawnPosition"] = json;
                    Manager.saveState["PlayerSpawnPosition"] = json;
                    PlayerPrefs.SetString("PlayerSpawnPosition", json);
                    break;
            }
        }

        GUIStyle guiStyle = new GUIStyle();

        private void OnDrawGizmos()
        {
            Collider2D[] collision = Physics2D.OverlapBoxAll(transform.position, new Vector2(1, 0.01f), 0);
            bool collided = false;
            foreach (Collider2D collision2d in collision)
            {
                if (!collision2d.isTrigger && !collision2d.CompareTag("Player"))
                {
                    collided = true;
                    break;
                }
            }

            Color color = collided ? Color.red : Color.green;
            Gizmos.color = color;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up);
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(-0.25f, 0.25f));
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0.25f, 0.25f));

            guiStyle.normal.textColor = color;
            guiStyle.alignment = TextAnchor.LowerCenter;
            Handles.Label(transform.position + Vector3.up, "�X�|�[���ʒu" + (collided ? "�i�Փ˒��j" : ""), guiStyle);
        }
    }
}

