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
    /**
     * Classes that derive from PersistentBehaviour will retain their state.
     * The exact length of persistence is determined by the persistence parameter.
     * If set to 'None', then this class will have no effect.
     * If set to 'Level', data will persist only if the same Level is reloaded
     * using LevelManager.ReloadLevel.
     * If set to 'Session', the data will persist until the application is 
     * exited.
     * If set to 'Save', the data is stored on disk and any changes will
     * persist even if the application is exited.
     * 
     * Classes that inherit from PersistentBehaviour do not need to implement
     * anything for this to work. All serializable properties will be remembered.
     */
    [RequireComponent(typeof(UniqueID))]
    public abstract class PersistentBehaviour : PuzzleBoxBehaviour
    {
        [SerializeField]
        [HideInInspector]
        private bool gameObjectActive = true;

        [SerializeField]
        [HideInInspector]
        private bool componentEnabled = true;

        // 状態をいつまで維持するか？
        public enum Persistence
        {
            None, // 維持しない
            Level, // 同じレベルがリロードされたら維持する
            Session, // アプリを終了しない限り維持する
            Save // 状態をセーブゲームに記録して維持する
        }

        public Persistence persistence = Persistence.None;

        public string uid { get; private set; }

        protected string persistenceKey
        {
            get
            {
                return "persistence::" + uid;
            }
        }

        protected void Awake()
        {
            uid = GetComponent<UniqueID>().uid;

            LoadState();
        }

        public void ClearSavedState()
        {
            string key = persistenceKey;

            if (LevelManager.saveState.ContainsKey(key))
            {
                LevelManager.saveState.Remove(key);
            }

            if (Manager.saveState.ContainsKey(key))
            {
                Manager.saveState.Remove(key);
            }

            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        public void SaveState()
        {
            string state = JsonUtility.ToJson(this);
            string key = persistenceKey;

            componentEnabled = enabled;
            gameObjectActive = gameObject.activeSelf;

            switch (persistence)
            {
                case Persistence.None:
                    break;
                case Persistence.Level:
                    LevelManager.saveState[key] = state;
                    break;
                case Persistence.Session:
                    Manager.saveState[key] = state;
                    break;
                case Persistence.Save:
                    PlayerPrefs.SetString(key, state);
                    break;
            }
        }

        public void LoadState(string json)
        {
            if (json != null && json != "")
            {
                JsonUtility.FromJsonOverwrite(json, this);

                enabled = componentEnabled;
                gameObject.SetActive(gameObjectActive);
            }
        }

        public void LoadState()
        {
            string key = persistenceKey;

            switch (persistence)
            {
                case Persistence.None:
                    return;
                case Persistence.Level:
                    if (LevelManager.saveState.ContainsKey(key))
                    {
                        LoadState(LevelManager.saveState[key]);
                    }
                    break;
                case Persistence.Session:
                    if (Manager.saveState.ContainsKey(key))
                    {
                        LoadState(Manager.saveState[key]);
                    }
                    break;
                case Persistence.Save:
                    if (PlayerPrefs.HasKey(key))
                    {
                        LoadState(PlayerPrefs.GetString(key));
                    }
                    break;
            }
        }
    }
}
