using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace PuzzleBox
{
    [RequireComponent(typeof(PuzzleBox.UniqueID))]
    public abstract class ValueBase : PuzzleBoxBehaviour
    {
        public Action OnValueChanged; 
    }
}
