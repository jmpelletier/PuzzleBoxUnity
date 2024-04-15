/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.ComponentModel;

namespace PuzzleBox
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class UniqueID : PuzzleBoxBehaviour
    {
        public string uid = "";

        // Start is called before the first frame update
        void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && uid == "")
            {
                uid = Guid.NewGuid().ToString();
                EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}

