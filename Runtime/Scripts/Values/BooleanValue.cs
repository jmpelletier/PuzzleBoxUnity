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
    public class BooleanValue : ValueBase
    {
        public bool value;

        protected override void OnEnable()
        {
            base.OnEnable();
            Set(value);
        }

        public void Set(bool newValue)
        {
            value = newValue;
            OnValueChanged?.Invoke();
            Save(value);
        }

        protected override void SilentlyUpdateValue(object newValue)
        {
            if (newValue is bool)
            {
                value = (bool)newValue;
            }
        }

        public override string ToString()
        {
            return value.ToString();
        }

        protected override void InitializeTarget()
        {
            Initialize<bool>();
        }
    }

    
}

