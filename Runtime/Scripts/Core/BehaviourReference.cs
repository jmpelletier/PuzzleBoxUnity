/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PuzzleBox
{
    [PuzzleBox.HideInEnumeration]
    public class BehaviourReference : PuzzleBoxBehaviour
    {
        [Space]
        public string path;

        [Tooltip("参照したいオブジェクトと同様のものをシーンまたはプレハブから選択できます。")]
        public GameObject sampleObject;

        [HideInInspector]
        public PuzzleBoxBehaviour puzzleBoxBehaviour = null;

        [HideInInspector]
        public string typeName = string.Empty;

        private System.Type type = null;

#if UNITY_EDITOR
        [HideInInspector]
        public MonoScript behaviour;
#endif

        private void FindType()
        {
            if (type == null)
            {
                if (!string.IsNullOrEmpty(typeName))
                {
                    if (type == null)
                    {
                        Assembly assembly = Assembly.GetExecutingAssembly();
                        type = assembly.GetType(typeName);
                    }
                }
            }
        }

        private void FindBehaviour(GameObject obj)
        {
            FindType();
            
            puzzleBoxBehaviour = null;
            if (type != null)
            {
                Component c = obj.GetComponentInChildren(type);
                puzzleBoxBehaviour = c as PuzzleBoxBehaviour;
            }
        }

        public void SetOwner(GameObject owner)
        {
            if (owner != null)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    Transform target = owner.transform.Find(path);
                    if (target != null)
                    {
                        FindBehaviour(target.gameObject);
                    }
                }
                else
                {
                    FindBehaviour(owner);
                }
            }
        }


        public override void Toggle(GameObject sender = null)
        {
            if (puzzleBoxBehaviour != null)
            {
                puzzleBoxBehaviour.Toggle();
            }
        }

        public override void Toggle(bool value)
        {
            if (puzzleBoxBehaviour != null)
            {
                puzzleBoxBehaviour.Toggle(value);
            }
        }

        public override bool GetToggleState()
        {
            if (puzzleBoxBehaviour != null)
            {
                return puzzleBoxBehaviour.GetToggleState();
            }
            return false;
        }

        [PuzzleBox.Action]
        public override void Enable(GameObject sender = null)
        {
            if (puzzleBoxBehaviour != null)
            {
                puzzleBoxBehaviour.Enable(sender);
            }
        }

        [PuzzleBox.Action]
        public override void Disable(GameObject sender = null)
        {
            if (puzzleBoxBehaviour != null)
            {
                puzzleBoxBehaviour.Disable(sender);
            }
        }

        public override void Invoke(string message, GameObject sender, GameObject[] arguments)
        {
            if (puzzleBoxBehaviour != null)
            {
                puzzleBoxBehaviour.Invoke(message, sender, arguments);
            }
        }
        public override string GetIcon()
        {
            return "referenceIcon";
        }

        public override void AddOverride(MonoBehaviour owner, string fieldName, object value, int priority)
        {
            if (puzzleBoxBehaviour != null)
            {
               puzzleBoxBehaviour.AddOverride(owner, fieldName, value, priority);
            }
        }

        public override void RemoveOverride(MonoBehaviour owner, string fieldName)
        {
            if (puzzleBoxBehaviour != null)
            {
                puzzleBoxBehaviour.RemoveOverride(owner, fieldName);
            }
        }

        public override void ApplyOverrides()
        {
            if (puzzleBoxBehaviour != null)
            {
                puzzleBoxBehaviour.ApplyOverrides();
            }
        }

        public override void RestoreOverrides()
        {
            if (puzzleBoxBehaviour != null) 
            {
                puzzleBoxBehaviour.RestoreOverrides();
            }
        }
    }
}

