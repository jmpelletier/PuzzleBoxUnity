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
    public class NumberValue : ValueBase
    {
        public float value;

        protected override void OnEnable()
        {
            base.OnEnable();
            Set(value);
        }

        public void Set(float newValue)
        {
            value = newValue;
            OnValueChanged?.Invoke();
            Save(value);
        }

        public void Set(int newValue)
        {
            value = newValue;
            OnValueChanged?.Invoke();
            Save(value);
        }

        protected override void SilentlyUpdateValue(object newValue)
        {
            if (newValue is int)
            {
                value = (int)newValue;
            }
            else if (newValue is float)
            {
                value = (float)newValue;
            }
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override void ValueChanged(int value)
        {
            Set(value);
        }

        public override void ValueChanged(float value)
        {
            Set(value);
        }

        protected override void InitializeTarget()
        {
            Initialize<float>();
        }
    }
}

