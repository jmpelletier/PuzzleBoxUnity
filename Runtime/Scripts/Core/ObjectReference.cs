/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace PuzzleBox
{
    [ExecuteAlways]
    [PuzzleBox.HideInEnumeration]
    public class ObjectReference : PuzzleBoxBehaviour
    {
        private GameObject _referencedObject = null;
        public GameObject referencedObject
        {
            get { return  _referencedObject; }
            set
            {
                _referencedObject = value;
                if (_referencedObject != null)
                {
                    behaviour = _referencedObject.GetComponent<PuzzleBoxBehaviour>();
                    foreach(BehaviourReference reference in referencedBehaviours)
                    {
                        if (reference != null)
                        {
                            reference.SetOwner(_referencedObject);
                        }
                    }
                }
                else
                {
                    behaviour = null;
                }
            }
        }

        public bool referenceChildBehaviours = true;
        public BehaviourReference[] referencedBehaviours;

        private PuzzleBoxBehaviour behaviour;

        public override void Invoke(string message, GameObject sender, GameObject[] arguments)
        {
            if (referencedObject != null)
            {
                if (behaviour == null || behaviour.gameObject != referencedObject)
                {
                    behaviour = referencedObject.GetComponent<PuzzleBoxBehaviour>();
                }
                if (behaviour != null)
                {
                    behaviour.Invoke(message, sender, arguments);
                }
            }
        }

        private void OnEnable()
        {
            UpdateReferences();
        }

        private void UpdateReferences()
        {
            if (referenceChildBehaviours)
            {
                if (referencedBehaviours == null)
                {
                    referencedBehaviours = GetComponentsInChildren<BehaviourReference>();
                }
                else
                {
                    referencedBehaviours = referencedBehaviours.Concat(GetComponentsInChildren<BehaviourReference>()).Where(x => x != null).Distinct().ToArray();
                }
            }
        }

        private void OnTransformChildrenChanged()
        {
            UpdateReferences();
        }

        public override string GetIcon()
        {
            return "referenceIcon";
        }
    }
}


