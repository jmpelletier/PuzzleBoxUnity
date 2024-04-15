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
    public abstract class PuzzleBoxBehaviour : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        public virtual string GetIcon()
        {
            return null;
        }

        public virtual void Toggle()
        {
            enabled = !enabled;
        }

        public virtual void Toggle(bool value)
        {
            enabled = value;
        }

        public virtual bool GetToggleState()
        {
            return isActiveAndEnabled;
        }

    }
}

