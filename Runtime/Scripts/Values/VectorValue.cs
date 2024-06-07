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
    public class VectorValue : ValueBase
    {
        public Vector3 value;

        protected override void OnEnable()
        {
            base.OnEnable();
            Set(value);
        }

        public void Set(Vector3 newValue)
        {
            value = newValue;
            OnValueChanged?.Invoke();
            Save(value);
        }

        protected override void SilentlyUpdateValue(object newValue)
        {
            if (newValue is Vector3)
            {
                value = (Vector3)newValue;
            }
        }

        public override string ToString()
        {
            return value.ToString();
        }

        protected override void InitializeTarget()
        {
            Initialize<Vector3>();
        }
    }
}

