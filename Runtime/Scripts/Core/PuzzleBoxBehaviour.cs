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
        [PuzzleBox.Overridable]
        public bool hideGizmo = false;

        private static Dictionary<string, MethodInfo[]> _actionMethodInfo = new Dictionary<string, MethodInfo[]>();
        private static void RegisterInstanceActions(PuzzleBoxBehaviour instance)
        {
            string type = instance.GetType().Name;
            if (!_actionMethodInfo.ContainsKey(type))
            {
                _actionMethodInfo[type] = ActionAttribute.GetMethods<ActionAttribute>(instance.GetType());
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

        protected struct Override
        {
            public MonoBehaviour owner;
            public FieldInfo field;
            public object value;
            public object defaultValue;
            public int priority;

            public static int Compare(Override a, Override b)
            {
                if (a.owner == null) return b.owner == null ? 0 : 1;
                else if (b.owner == null) return -1;
                else return a.priority - b.priority;
            }
        }

        private const int MAX_OVERRIDES = 8;

        public virtual void AddOverride(MonoBehaviour owner, string fieldName, object value, int priority)
        {
            // Make sure field can be overridden
            if (overrides.ContainsKey(fieldName))
            {
                Override[] fieldOverrides = overrides[fieldName];

                // See if override already exist
                for (int i = 0; i < MAX_OVERRIDES; i++)
                {
                    if (fieldOverrides[i].owner == null)
                    {
                        break;
                    }
                    else
                    {
                        if (fieldOverrides[i].owner == owner && fieldOverrides[i].field.Name.Equals(fieldName))
                        {
                            // Remove so we can add again
                            RemoveOverride(owner, fieldName);
                        }
                    }
                }

                // See if already full
                if (fieldOverrides[MAX_OVERRIDES - 1].owner != null)
                {
                    return; // Full...
                }

                // Store the default value
                FieldInfo fieldInfo = _overridableFields[GetType()][fieldName];
                // If there are overrides active, this may not be correct.
                // We will need to obtain the correct value from other overrides.
                object defaultValue = fieldInfo.GetValue(this); 

                // Add override at the end
                for (int i = 0; i < MAX_OVERRIDES; i++)
                {
                    if (fieldOverrides[i].owner == null)
                    {
                        fieldOverrides[i].owner = owner;
                        fieldOverrides[i].field = fieldInfo;
                        fieldOverrides[i].value = value;
                        fieldOverrides[i].defaultValue = defaultValue;
                        fieldOverrides[i].priority = priority;

                        break;
                    }
                    else
                    {
                        // We get previous overrides to teach us the actual default value.
                        defaultValue = fieldOverrides[i].defaultValue;
                    }
                }

                // Now sort according to priority
                Array.Sort(fieldOverrides, Override.Compare);

                ApplyOverrides();
            }
        }

        public virtual void RemoveOverride(MonoBehaviour owner, string fieldName)
        {
            if (overrides.ContainsKey(fieldName))
            {
                Override[] fieldOverrides = overrides[fieldName];

                for (int i = 0; i < MAX_OVERRIDES; i++)
                {
                    if (fieldOverrides[i].owner == null)
                    {
                        return;
                    }

                    if (fieldOverrides[i].owner == owner)
                    {
                        if (i == 0 && fieldOverrides[1].owner == null)
                        {
                            // This is the last override for this field.
                            // Restore defaults.
                            fieldOverrides[i].field.SetValue(this, fieldOverrides[i].defaultValue);
                            fieldOverrides[i].owner = null;
                            return; // No need to check the rest
                        }

                        for (int j = i + 1; j < MAX_OVERRIDES; j++, i++)
                        {
                            fieldOverrides[i] = fieldOverrides[j];
                        }
                        fieldOverrides[MAX_OVERRIDES - 1].owner = null;
                    }
                }

                ApplyOverrides();
            }
        }


        private void InitializeOverrides()
        {
            if (!_overridableFields.ContainsKey(GetType()))
            {
                Dictionary<string, FieldInfo> fieldMap = new Dictionary<string, FieldInfo>();
                FieldInfo[] fields = FieldAttribute.GetFields<OverridableAttribute>(GetType());
                foreach (FieldInfo field in fields)
                {
                    fieldMap[field.Name] = field;
                }
                _overridableFields[GetType()] = fieldMap;
            }

            foreach(var pair in _overridableFields[GetType()])
            {
                overrides[pair.Key] = new Override[MAX_OVERRIDES];
            }
        }

        protected Dictionary<string, Override[]> overrides { get; private set; } = new Dictionary<string, Override[]>();
        protected static Dictionary<Type, Dictionary<string, FieldInfo>> _overridableFields = new Dictionary<Type, Dictionary<string, FieldInfo>>();  

        public virtual void ApplyOverrides()
        {
            foreach(var pair in overrides)
            {
                for (int i = 0; i < MAX_OVERRIDES; i++)
                {
                    if (pair.Value[i].owner == null) break;
                    else
                    {
                        pair.Value[i].field.SetValue(this, pair.Value[i].value);
                    }
                }
            }
        }

        public virtual void RestoreOverrides()
        {
            foreach (var pair in overrides)
            {
                if (pair.Value[0].owner != null) // All default values are the same.
                {
                    // Note that if an overridden field is directly modified,
                    // this change will be undone when RestoreOverrides is called.
                    // In practice, this should probably not happen too often,
                    // but it is a possibility.
                    pair.Value[0].field.SetValue(this, pair.Value[0].defaultValue);
                }
            }
        }


        public PuzzleBoxBehaviour() : base()
        {
            RegisterInstanceActions(this);
            InitializeOverrides();
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

