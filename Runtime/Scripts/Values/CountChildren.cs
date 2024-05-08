/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    public class CountChildren : PuzzleBoxBehaviour
    {
        public int targetCount = 0;

        [Space]
        public ActionDelegate[] reachedCount;


        // Start is called before the first frame update
        void Start()
        {
            OnTransformChildrenChanged();
        }

        private void OnTransformChildrenChanged()
        {
            int count = transform.childCount;
            if (count == targetCount)
            {
                foreach (ActionDelegate actionDelegate in reachedCount)
                {
                    if (actionDelegate != null)
                    {
                        actionDelegate.Perform(gameObject);
                    }
                }
            }
        }

        public override string GetIcon()
        {
            return "counterIcon";
        }
    }
}

