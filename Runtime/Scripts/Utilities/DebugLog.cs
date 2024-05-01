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
    public class DebugLog : PuzzleBoxBehaviour
    {
        public string message;

        [PuzzleBox.Action]
        public void Print(GameObject sender, GameObject[] arguments)
        {
            Debug.Log($"Message from {sender.name}: {message}");
            ForEach<PuzzleBoxBehaviour>(arguments, x => Debug.Log($"{x.GetType().Name}: {x.ToString()}"));
        }

        public override string GetIcon()
        {
            return "DebugIcon";
        }
    }
}

