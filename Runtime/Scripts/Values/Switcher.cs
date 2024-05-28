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
    public class Switcher : PuzzleBoxBehaviour
    {
        [System.Serializable]
        public struct Range
        {
            public float max;
            public float min;
            public GameObject target;
        }

        public NumberValue value;

        public Range[] ranges;

        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnEnable()
        {
            if (value != null)
            {
                value.Subscribe(UpdateTargets);
            }
        }

        private void OnDisable()
        {
            if (value != null)
            {
                value.Unsubscribe(UpdateTargets);
            }
        }

        private void UpdateTargets()
        {
            if (value != null && ranges != null)
            {
                foreach(Range range in ranges)
                {
                    if (range.target != null)
                    {
                        range.target.SetActive(value.value < range.max && value.value >= range.min);
                    }
                }
            }
        }
    }
}

