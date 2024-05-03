/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using UnityEngine;

namespace PuzzleBox
{
    [DisallowMultipleComponent]
    public abstract class ActionDelegate : PuzzleBoxBehaviour
    {
        [Min(0)]
        public float delay = 0f;
        public virtual void Perform(GameObject sender) { }
        public virtual void Perform(GameObject sender, bool value) { }
        public virtual void Perform(GameObject sender, float value) { }
        public virtual void Perform(GameObject sender, Vector2 value) { }
        public virtual void Perform(GameObject sender, Vector3 value) { }
        public virtual void Perform(GameObject sender, GameObject target) { }

        public virtual void Pause() { }

        [System.Serializable]
        public struct Target
        {
            public GameObject target;
            public PuzzleBoxBehaviour behaviour;
            public string methodName;
        }

        protected void PerformAction(Action action)
        {
            if (delay > 0)
            {
                StartCoroutine(PerformActionWithDelay(action));
            }
            else
            {
                action?.Invoke();
            }
        }

        private IEnumerator PerformActionWithDelay(Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

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

