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

        [SerializeField]
        [HideInInspector]
        protected bool deleted = false;

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

        protected virtual void Awake()
        {
            uid = GetComponent<UniqueID>().uid;

            LoadState();
        }

        public void ClearSavedState()
        {
            string key = persistenceKey;

            if (LevelManager.temporarySaveState.ContainsKey(key))
            {
                LevelManager.temporarySaveState.Remove(key);
            }

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

        public static void Save(string key, MonoBehaviour behaviour, Persistence persistence)
        {
            if (behaviour != null)
            {
                string state = JsonUtility.ToJson(behaviour);

                switch (persistence)
                {
                    case Persistence.None:
                        LevelManager.temporarySaveState.Set(key, state);
                        break;
                    case Persistence.Level:
                        LevelManager.saveState.Set(key, state);
                        break;
                    case Persistence.Session:
                        Manager.saveState.Set(key, state);
                        break;
                    case Persistence.Save:
                        Manager.WriteToSaveGame(key, state);
                        break;
                }
            }
        }

        public static void Load(string key, MonoBehaviour behaviour)
        {
            if (behaviour != null)
            {
                string json = null;

                if (LevelManager.temporarySaveState.ContainsKey(key))
                {
                    json = LevelManager.temporarySaveState.Get<string>(key);
                }
                else if (LevelManager.saveState.ContainsKey(key))
                {
                    json = LevelManager.saveState.Get<string>(key);
                }
                else if (Manager.saveState.ContainsKey(key))
                {
                    json = Manager.saveState.Get<string>(key);
                }

                if (!string.IsNullOrEmpty(json))
                {
                    JsonUtility.FromJsonOverwrite(json, behaviour);
                }
            }
        }

        public static void Load(string key, PersistentBehaviour behaviour)
        {
            if (behaviour != null)
            {
                Load(key, (MonoBehaviour)behaviour);
                if (behaviour.deleted)
                {
                    Destroy(behaviour.gameObject);
                }
                else
                {
                    behaviour.enabled = behaviour.componentEnabled;
                    behaviour.gameObject.SetActive(behaviour.gameObjectActive);
                }
            }
        }

        public void SaveState()
        {
            componentEnabled = enabled;
            gameObjectActive = gameObject.activeSelf;

            Save(persistenceKey, this, persistence);
        }

        public void LoadState()
        {
            Load(persistenceKey, this);
        }

        protected virtual void WasDestroyed()
        {
            deleted = true;
            SaveState();
        }
    }
}
