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
    public class ObjectLifetime : PersistentBehaviour
    {
        public ActionDelegate[] OnStart;
        public ActionDelegate[] OnDestroyed;

        // Start is called before the first frame update
        void Start()
        {
            foreach (ActionDelegate action in OnStart)
            {
                if (action != null)
                {
                    action.Perform(gameObject);
                }
            }

            SaveState();
        }

        protected override void WasDestroyed()
        {
            foreach (ActionDelegate action in OnDestroyed)
            {
                if (action != null)
                {
                    action.Perform(gameObject);
                }
            }

            base.WasDestroyed();
        }
    }

}
