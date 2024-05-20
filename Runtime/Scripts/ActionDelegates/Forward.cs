/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using UnityEngine;

namespace PuzzleBox
{
    public class Forward : ActionDelegate
    {
        private ActionDelegate[] actionDelegates = new ActionDelegate[0];

        private void OnTransformChildrenChanged()
        {
            actionDelegates = GetComponentsInChildren<ActionDelegate>();
        }

        public override void Perform(GameObject sender) 
        {
            PerformAction(() => Invoke(actionDelegates, sender));
        }

        public override void Perform(GameObject sender, bool value)
        {
            PerformAction(() => Invoke(actionDelegates, sender, value));
        }

        public override void Perform(GameObject sender, float value)
        {
            PerformAction(() => Invoke(actionDelegates, sender, value));
        }

        public override void Perform(GameObject sender, Vector2 value)
        {
            PerformAction(() => Invoke(actionDelegates, sender, value));
        }

        public override void Perform(GameObject sender, GameObject target)
        {
            PerformAction(() => Invoke(actionDelegates, sender, target));
        }
    }
}

