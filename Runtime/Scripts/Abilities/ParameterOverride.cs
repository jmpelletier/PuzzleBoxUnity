/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;

namespace PuzzleBox
{
    public class ParameterOverride : PuzzleBoxBehaviour
    {
        [System.Serializable]
        public struct Value
        {
            public string type;
            public int intValue;
            public float floatValue;
            public bool boolValue;
            public string stringValue;
            public Vector2 vector2Value;
            public Vector3 vector3Value;

            public object GetObject()
            {
                if (type == typeof(int).Name) return intValue;
                else if (type == typeof(float).Name) return floatValue;
                else if (type == typeof(bool).Name) return boolValue;
                else if (type == typeof(string).Name) return stringValue;
                else if (type == typeof(Vector2).Name) return vector2Value;
                else if (type == typeof(Vector3).Name) return vector3Value;
                else return null;
            }

            public override string ToString()
            {
                object obj = GetObject();
                return obj != null ? obj.ToString() : string.Empty;
            }
        }

        public int priority = 0;

        public ActionDelegate.Target target;

        [HideInInspector]
        public string fieldName = "";

        [HideInInspector]
        public Value value;

        // Start is called before the first frame update
        void Start()
        {
            
        }


        private void OnEnable()
        {
            if (target.behaviour != null)
            {
                target.behaviour.AddOverride(this, fieldName, value.GetObject(), priority);
            }
        }

        private void OnDisable()
        {
            if (target.behaviour != null)
            {
                target.behaviour.RemoveOverride(this, fieldName);
            }
        }

        public override string GetIcon()
        {
            return "parameterOverrideIcon";
        }
    }
}

