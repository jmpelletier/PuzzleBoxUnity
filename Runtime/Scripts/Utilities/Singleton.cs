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
    // Singletonが必ず他のスクリプトよりも先に実行されるように指定する。
    // 特に「EventSystem」の実行順番が低いので、以下の設定を行わないと
    // Singletonを使用しても警告が出てしまう。
    [DefaultExecutionOrder(int.MinValue)]
    public class Singleton : Utility
    {
        private static Dictionary<string, Singleton> instances = new Dictionary<string, Singleton>();

        public string id = "";
        public bool dontDestroyOnLoad = false;

        private void Awake()
        {
            if (instances.ContainsKey(id))
            {
                if (instances[id] != null && instances[id].dontDestroyOnLoad)
                {
                    Destroy(gameObject);
                    return;
                }
                else if (instances[id] != null)
                {
                    Destroy(instances[id].gameObject);
                }
            }

            instances[id] = this;

            if (dontDestroyOnLoad)
            {
                transform.parent = null;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnEnable()
        {
            if (instances.ContainsKey(id) && instances[id] != this)
            {
                gameObject.SetActive(false);
            }
        }
    }
}

