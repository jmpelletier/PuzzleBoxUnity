using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace PuzzleBox
{
    public class StringValue : ValueBase
    {
        public string value;

        public void OnEnable()
        {
            Set(value);
        }

        public void Set(string newValue)
        {
            value = newValue;
            OnValueChanged?.Invoke();
        }
        public override string ToString()
        {
            return value;
        }
    }
}

