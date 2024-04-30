/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
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
        public MonoScript behaviour;

        [HideInInspector]
        public PuzzleBoxBehaviour puzzleBoxBehaviour = null;

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

        private void FindBehaviour(GameObject obj)
        {
            puzzleBoxBehaviour = null;
            if (behaviour != null)
            {
                Component c = obj.GetComponentInChildren(behaviour.GetClass());
                puzzleBoxBehaviour = c as PuzzleBoxBehaviour;
            }
        }

        public override void Toggle()
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
        public override void Enable()
        {
            if (puzzleBoxBehaviour != null)
            {
                puzzleBoxBehaviour.Enable();
            }
        }

        [PuzzleBox.Action]
        public override void Disable()
        {
            if (puzzleBoxBehaviour != null)
            {
                puzzleBoxBehaviour.Disable();
            }
        }

        public override void Invoke(string message)
        {
            if (puzzleBoxBehaviour != null)
            {
                puzzleBoxBehaviour.Invoke(message);
            }
        }
    }
}

