/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using PuzzleBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace PuzzleBox
{
    [DefaultExecutionOrder(int.MinValue + 2)]
    public class LevelManager : PuzzleBoxBehaviour
    {
        public GameObject player;

        public static DynamicDictionary saveState = new DynamicDictionary();
        public static DynamicDictionary temporarySaveState = new DynamicDictionary();

        public static string mainScene = "";
        public static string subScene = "";

        public static List<LevelManager> instances = new List<LevelManager>();
        public static LevelManager instance
        {
            get
            {
                if (instances.Count == 0) return null;
                else
                {
                    return instances[instances.Count - 1];
                }
            }
        }


        void SetupPlayer()
        {
            string stateJson = GetSaveState("PlayerSpawnPosition");
            if (stateJson != "")
            {
                Vector3 p = JsonUtility.FromJson<Vector3>(stateJson);
                player.transform.position = p;
            }
            else
            {
                string spawnPointUid = GetSaveState("SpawnPointUid");
                if (spawnPointUid != "")
                {
                    GameObject go = FindGameObjectByUniqueId(spawnPointUid);
                    if (go != null)
                    {
                        player.transform.position = go.transform.position;
                    }
                }
            }

            ListenToPlayerActions();
        }

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(subScene))
            {
                subScene = gameObject.scene.name;
            }
            instances.Add(this);
        }

        protected virtual void OnDestroy()
        {
            instances.Remove(this);
        }

        protected string GetSaveState(string key)
        {
            if (temporarySaveState.ContainsKey(key))
            {
                return temporarySaveState.Get<string>(key);
            }

            if (saveState.ContainsKey(key))
            {
                return saveState.Get<string>(key);
            }

            if (Manager.saveState.ContainsKey(key))
            {
                return Manager.saveState.Get<string>(key);
            }

            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }

            return "";
        }

        // Start is called before the first frame update
        void Start()
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }

            if (player != null)
            {
                SetupPlayer();
            }
        }

        public bool isPaused {  get; protected set; }


        public virtual void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;

            if (player != null)
            {
                player.SendMessage("SetUserInputEnabled", false);
            }
        }

        public virtual void Resume()
        {
            isPaused = false;
            Time.timeScale = 1f;

            if (player != null)
            {
                player.SendMessage("SetUserInputEnabled", true);
            }
        }

        protected void OnPause()
        {
            if (isPaused)
            {
                if (Manager.instance != null)
                {
                    Manager.instance.Unpause();
                }
            }
            else
            {
                if (Manager.instance != null)
                {
                    Manager.instance.Pause();
                }
            }
            
        }

        private void ListenToPlayerActions()
        {
            if (player != null)
            {
                LifetimeActions lifetimeActions = player.GetComponent<LifetimeActions>();
                if (lifetimeActions != null)
                {
                    lifetimeActions.OnKilled += obj => ReloadLevel();
                }
            }
        }

        public static void UnloadSubScene()
        {
            if (!string.IsNullOrEmpty(subScene))
            {
                SceneTransition.UnloadScene(subScene);
            }
        }

        public void ReloadLevel()
        {
            temporarySaveState.Clear();

            if (string.IsNullOrEmpty(mainScene))
            {
                SceneTransition.ReloadCurrentScene();
            }
            else
            {
                LoadLevel(subScene, "", false);
            }
        }

        static void saveSpawnPoint(string spawnPointUID)
        {
            if (!string.IsNullOrEmpty(spawnPointUID))
            {
                saveState.Set("SpawnPointUid", spawnPointUID);
            }
        }

        public static void ClearSaveState()
        {
            saveState.Clear();
        }

        public static void LoadLevel(string sceneName, string spawnPointUID = "", bool clearState = true)
        {
            if (string.IsNullOrEmpty(mainScene))
            {
                if (clearState)
                {
                    saveState.Clear();
                }
                saveSpawnPoint(spawnPointUID);
                SceneTransition.LoadScene(sceneName);
            }
            else
            {
                LoadLevelAdditive(sceneName, spawnPointUID, clearState);
            }

            subScene = sceneName;
        }

        public static void LoadLevelAdditive(string sceneName, string spawnPointUID = "", bool clearState = true)
        {
            if (clearState)
            {
                saveState.Clear();
            }
            saveSpawnPoint(spawnPointUID);

            UnloadSubScene();

            SceneTransition.LoadScene(sceneName, true);

            subScene = sceneName;
        }

        public static void LoadLevelAsync(string sceneName, Action<bool> onDone, string spawnPointUID = "", bool clearState = true)
        {
            if (instance == null)
            {
                Debug.LogError("LevelManagerのインスタンスが見つからない。必ずシーンにLevelManagerを追加してください。");
                onDone?.Invoke(false);
                return;
            }

            instance.StartCoroutine(instance.PerformLoadLevelAsync(sceneName, onDone, spawnPointUID, clearState));
        }

        IEnumerator PerformLoadLevelAsync(string sceneName, Action<bool> onDone, string spawnPointUID = "", bool clearState = true)
        {
            AsyncOperation asyncLoad;
            if (!string.IsNullOrEmpty(mainScene))
            {
                asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            }
            else
            {
                asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }

            while(!asyncLoad.isDone)
            {
                yield return null;
            }

            if (clearState)
            {
                saveState.Clear();
            }
            saveSpawnPoint(spawnPointUID);

            subScene = sceneName;

            onDone?.Invoke(true);
        }

        public GameObject FindGameObjectByUniqueId(string uid)
        {
            UniqueID[] uids = FindObjectsOfType<UniqueID>();
            foreach (UniqueID uniqueID in uids)
            {
                if (uniqueID.uid == uid)
                {
                    return uniqueID.gameObject;
                }
            }
            return null;
        }
    }
}

