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
    public class ObjectReference : PuzzleBoxBehaviour
    {
        [HideInInspector]
        public GameObject referencedObject = null;
        private PuzzleBoxBehaviour behaviour;

        public override void Invoke(string message)
        {
            if (referencedObject != null)
            {
                if (behaviour == null || behaviour.gameObject != referencedObject)
                {
                    behaviour = referencedObject.GetComponent<PuzzleBoxBehaviour>();
                }
                if (behaviour != null)
                {
                    behaviour.Invoke(message);
                }
            }
        }
    }
}


