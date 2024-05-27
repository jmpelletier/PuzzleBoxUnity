/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    [DefaultExecutionOrder(int.MinValue + 1)]
    public class Manager : MonoBehaviour
    {
        public static Manager instance;

        public static DynamicDictionary saveState = new DynamicDictionary();

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                LevelManager.mainScene = gameObject.scene.name;
            }
        }

        public static string Serialize(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            if (obj.GetType().IsPrimitive)
            {
                return obj.ToString();
            }

            else
            {
                return JsonUtility.ToJson(obj);
            }
        }

        public static T Parse<T>(string str, T defaultValue = default(T)) where T : struct
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (typeof(T).IsPrimitive)
                {
                    try
                    {
                        return (T)Convert.ChangeType(str, typeof(T));
                    }
                    catch
                    {
                        return defaultValue;
                    }
                }
                else
                {
                    try
                    {
                        return JsonUtility.FromJson<T>(str);
                    }
                    catch (Exception)
                    {
                        return defaultValue;
                    }
                }

            }
            return defaultValue;
        }

        public static void WriteToSaveGame(string key, string value)
        {
            saveState.Set(key, value);
            PlayerPrefs.SetString(key, value);
        }

        public static T ReadFromSaveGame<T>(string key, T defaultValue = default(T)) where T : struct
        {
            if (saveState.ContainsKey(key))
            {
                return saveState.Get<T>(key, defaultValue);
            }
            else if (PlayerPrefs.HasKey(key))
            {
                string str = PlayerPrefs.GetString(key);
                return Parse<T>(str, defaultValue);
            }
            else
            {
                string typedKey = MakeTypedKey(key, typeof(T));
                if (PlayerPrefs.HasKey(typedKey))
                {
                    string str = PlayerPrefs.GetString(typedKey);
                    return Parse<T>(str, defaultValue);
                }
            }

            return defaultValue;
        }

        public static string ReadStringFromSaveGame(string key, string defaultValue = default(string))
        {
            if (saveState.ContainsKey(key))
            {
                return saveState.Get<string>(key, defaultValue);
            }
            else if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }
            else
            {
                string typedKey = MakeTypedKey(key, typeof(string));
                if (PlayerPrefs.HasKey(typedKey))
                {
                    return PlayerPrefs.GetString(typedKey);
                }
            }

            return defaultValue;
        }

        private static string MakeTypedKey(string key, Type type)
        {
            return $"{type.Name}::{key}";
        }


        public static void WriteToSaveGame(string key, object value)
        {
            saveState.Set(key, value);
            string typedKey = MakeTypedKey(key, value.GetType());
            string str = Serialize(value);
            PlayerPrefs.SetString(typedKey, str);
        }

        public virtual void ConfirmQuitGame()
        {
            
        }

        public virtual void CancelQuitGame()
        {
            
        }

        public virtual void QuitGame()
        {
            Application.Quit();
        }

        public virtual void ClearGame()
        {

        }

        public virtual void StartPlay()
        {

        }

        public virtual void EndPlay()
        {
            // 停止しているかも知れないので元に戻す。
            Time.timeScale = 1f;
        }

        public virtual void Pause()
        {
            
        }

        public virtual void Unpause()
        {

        }

        public virtual void ShowTitle()
        {

        }

        
    }
}

