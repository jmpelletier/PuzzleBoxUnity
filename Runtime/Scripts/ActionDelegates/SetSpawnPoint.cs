/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
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
            PerformAction(() => {
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
            });
        }
    }
}

