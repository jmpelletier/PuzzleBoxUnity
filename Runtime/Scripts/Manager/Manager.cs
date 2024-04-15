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
    public class Manager : MonoBehaviour
    {
        public static Manager instance;

        public static Dictionary<string, string> saveState = new Dictionary<string, string>();

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning("余分のManagerを削除する。");
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                LevelManager.mainScene = gameObject.scene.name;
            }
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

