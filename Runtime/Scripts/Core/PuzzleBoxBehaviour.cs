/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace PuzzleBox
{
    public abstract class PuzzleBoxBehaviour : MonoBehaviour
    {
        public bool hideGizmo = false;

        private static Dictionary<string, MethodInfo[]> _actionMethodInfo = new Dictionary<string, MethodInfo[]>();
        private static void RegisterInstanceActions(PuzzleBoxBehaviour instance)
        {
            string type = instance.GetType().Name;
            if (!_actionMethodInfo.ContainsKey(type))
            {
                _actionMethodInfo[type] = ActionAttribute.GetMethods(instance.GetType());
            }

            foreach(MethodInfo method in _actionMethodInfo[type])
            {
                instance._actions[method.Name] = (Action)Delegate.CreateDelegate(typeof(Action), instance, method);
            }
        }

        protected Dictionary<string, Action> _actions { get; private set; } = new Dictionary<string, Action>();

        public PuzzleBoxBehaviour() : base()
        {
            RegisterInstanceActions(this);
        }
         

        // Start is called before the first frame update
        void Start()
        {

        }

        public virtual string GetIcon()
        {
            return null;
        }

        public virtual void Toggle()
        {
            enabled = !enabled;
        }

        public virtual void Toggle(bool value)
        {
            enabled = value;
        }

        public virtual bool GetToggleState()
        {
            return isActiveAndEnabled;
        }

        [PuzzleBox.Action]
        public void Enable()
        {
            enabled = true;
        }

        [PuzzleBox.Action]
        public void Disable()
        {
            enabled = false;
        }

        public virtual void Invoke(string message)
        {
            if (_actions.ContainsKey(message))
            {
                Action action = _actions[message];
                if (action != null)
                {
                    action.Invoke();
                }
            }
        }

    }
}

