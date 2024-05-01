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
                Action<GameObject, GameObject[]> action = instance.MakeAction(method);

                if (action != null)
                {
                    instance._actions[method.Name] = action;
                }
                else
                {
                    Debug.LogError($"Invalid PuzzleBox.Action ({method.DeclaringType}.{method.Name}): check return and argument types.");
                }
            }
        }

        private Action<GameObject, GameObject[]> MakeAction(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (method.ReturnType == typeof(void))
            {
                if (parameters.Length == 2 &&
                        parameters[0].ParameterType == typeof(GameObject) &&
                        parameters[1].ParameterType == typeof(GameObject[]))
                {
                    return (Action<GameObject, GameObject[]>)Delegate.CreateDelegate(typeof(Action<GameObject, GameObject[]>), this, method);
                }
                else if (parameters.Length == 1 &&
                        parameters[0].ParameterType == typeof(GameObject))
                {
                    return (sender, args) => { method.Invoke(this, new object[] { sender }); };
                }
                else if (parameters.Length == 0)
                {
                    return (sender, args) => { method.Invoke(this, new object[] { }); };
                }
            }

            return null;
        }

        protected Dictionary<string, Action<GameObject, GameObject[]>> _actions { get; private set; } = new Dictionary<string, Action<GameObject, GameObject[]>>();

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
        public virtual void Enable()
        {
            enabled = true;
        }

        [PuzzleBox.Action]
        public virtual void Disable()
        {
            enabled = false;
        }

        public virtual void Invoke(string message, GameObject sender, GameObject[] arguments)
        {
            if (_actions.ContainsKey(message))
            {
                Action<GameObject, GameObject[]> action = _actions[message];
                if (action != null)
                {
                    action.Invoke(sender, arguments);
                }
            }
        }

        protected static void ForEach<T>(GameObject gameObject, Action<T> action)
        {
            if (gameObject != null && action != null)
            {
                foreach (T behaviour in gameObject.GetComponentsInChildren<T>())
                {
                    if (behaviour != null)
                    {
                        action.Invoke(behaviour);
                    }
                }
            }
        }

        protected static void ForEach<T>(GameObject[] gameObjects, Action<T> action)
        {
            if (gameObjects != null && action != null)
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    ForEach<T>(gameObject, action);
                }
            }
        }
    }
}

