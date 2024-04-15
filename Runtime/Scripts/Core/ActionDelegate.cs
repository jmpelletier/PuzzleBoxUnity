/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using UnityEngine;

namespace PuzzleBox
{
    [DisallowMultipleComponent]
    public abstract class ActionDelegate : PuzzleBoxBehaviour
    {
        public virtual void Perform(GameObject sender) { }
        public virtual void Perform(GameObject sender, bool value) { }
        public virtual void Perform(GameObject sender, float value) { }
        public virtual void Perform(GameObject sender, Vector2 value) { }
        public virtual void Perform(GameObject sender, Vector3 value) { }
        public virtual void Perform(GameObject sender, GameObject target) { }

        public virtual void Pause() { }

        public static void Invoke(ActionDelegate[] delegates, GameObject sender)
        {
            foreach(ActionDelegate action in delegates)
            {
                if (action)
                {
                    action.Perform(sender);
                }
            }
        }

        public static void Invoke(ActionDelegate[] delegates, GameObject sender, bool value)
        {
            foreach (ActionDelegate action in delegates)
            {
                if (action)
                {
                    action.Perform(sender, value);
                }
            }
        }

        public static void Invoke(ActionDelegate[] delegates, GameObject sender, float value)
        {
            foreach (ActionDelegate action in delegates)
            {
                if (action)
                {
                    action.Perform(sender, value);
                }
            }
        }

        public static void Invoke(ActionDelegate[] delegates, GameObject sender, Vector2 value)
        {
            foreach (ActionDelegate action in delegates)
            {
                if (action)
                {
                    action.Perform(sender, value);
                }
            }
        }

        public static void Invoke(ActionDelegate[] delegates, GameObject sender, Vector3 value)
        {
            foreach (ActionDelegate action in delegates)
            {
                if (action)
                {
                    action.Perform(sender, value);
                }
            }
        }

        public static void Invoke(ActionDelegate[] delegates, GameObject sender, GameObject value)
        {
            foreach (ActionDelegate action in delegates)
            {
                if (action)
                {
                    action.Perform(sender, value);
                }
            }
        }

        public override string GetIcon()
        {
            return "ActionIcon";
        }
    }
}

