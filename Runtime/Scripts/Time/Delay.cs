/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;

namespace PuzzleBox
{
    [PuzzleBox.HideInEnumeration]
    [ExecuteAlways]
    public class Delay : ActionDelegate
    {
        [Min(0)]
        public float delay = 0f;
        public bool useFixedUpdate = true;
        public bool autoAddChildren = true;

        public List<ActionDelegate> targets = new List<ActionDelegate>();

        private float time
        {
            get
            {
                return useFixedUpdate ? Time.fixedTime : Time.time;
            }
        }

        private float executionTime
        {
            get
            {
                return time + delay;
            }
        }

        private class DelayedAction
        {
            public float time;
            public GameObject sender;
            public DelayedAction(float time, GameObject sender)
            {
                this.sender = sender;
                this.time = time;
            }
        }

        private class DelayedBoolAction : DelayedAction
        {
            public bool value;
            public DelayedBoolAction(float time, GameObject sender, bool value) : base(time, sender)
            {
                this.value = value;
            }
        }

        private class DelayedFloatAction : DelayedAction
        {
            public float value;
            public DelayedFloatAction(float time, GameObject sender, float value) : base(time, sender)
            {
                this.value = value;
            }
        }

        private class DelayedVector2Action : DelayedAction
        {
            public Vector2 value;
            public DelayedVector2Action(float time, GameObject sender, Vector2 value) : base(time, sender)
            {
                this.value = value;
            }
        }

        private class DelayedVector3Action : DelayedAction
        {
            public Vector3 value;
            public DelayedVector3Action(float time, GameObject sender, Vector3 value) : base(time, sender)
            {
                this.value = value;
            }
        }

        private class DelayedGameObjectAction : DelayedAction
        {
            public GameObject value;
            public DelayedGameObjectAction(float time, GameObject sender, GameObject value) : base(time, sender)
            {
                this.value = value;
            }
        }

        private LinkedList<DelayedAction> actions = new LinkedList<DelayedAction>();

        // Start is called before the first frame update
        void Start()
        {

        }

        private void PerformAction(DelayedAction action)
        {
            if (targets.Count == 0)
            {
                return;
            }

            if (action is DelayedBoolAction)
            {
                foreach(ActionDelegate actionDelegate in targets)
                {
                    actionDelegate.Perform(action.sender, ((DelayedBoolAction)action).value);
                }
            }
            else if (action is DelayedFloatAction)
            {
                foreach (ActionDelegate actionDelegate in targets)
                {
                    actionDelegate.Perform(action.sender, ((DelayedFloatAction)action).value);
                }
            }
            else if (action is DelayedVector2Action)
            {
                foreach (ActionDelegate actionDelegate in targets)
                {
                    actionDelegate.Perform(action.sender, ((DelayedVector2Action)action).value);
                }
            }
            else if (action is DelayedVector3Action)
            {
                foreach (ActionDelegate actionDelegate in targets)
                {
                    actionDelegate.Perform(action.sender, ((DelayedVector3Action)action).value);
                }
            }
            else if (action is DelayedGameObjectAction)
            {
                foreach (ActionDelegate actionDelegate in targets)
                {
                    actionDelegate.Perform(action.sender, ((DelayedGameObjectAction)action).value);
                }
            }
            else
            {
                foreach (ActionDelegate actionDelegate in targets)
                {
                    actionDelegate.Perform(action.sender);
                }
            }
        }

        private void PerformUpdate()
        {
            if (!Application.isPlaying) return;

            float now = time;
            for (LinkedListNode<DelayedAction> node = actions.First;  node != null; node = node.Next)
            {
                if (node.Value.time <= now)
                {
                    PerformAction(node.Value);
                    LinkedListNode<DelayedAction> next = node.Next;
                    actions.Remove(node);
                    node = next;
                    if (node == null)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void AddAction(DelayedAction action)
        {
            for (LinkedListNode<DelayedAction> node = actions.Last; node != null; node = node.Previous)
            {
                if (node.Value.time < action.time)
                {
                    actions.AddAfter(node, action);
                    return;
                }
            }

            actions.AddLast(action);
        }

        void FixedUpdate()
        {
            if (useFixedUpdate)
            {
                PerformUpdate();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!useFixedUpdate)
            {
                PerformUpdate();
            }
        }

        private void OnTransformChildrenChanged()
        {
            if (autoAddChildren)
            {
                ActionDelegate[] childDelegates = GetComponentsInChildren<ActionDelegate>();
                foreach(ActionDelegate childDelegate in childDelegates)
                {
                    if (childDelegate == this)
                    {
                        continue;
                    }

                    bool alreadyAdded = false;
                    foreach(ActionDelegate actionDelegate in targets)
                    {
                        if (actionDelegate == childDelegate)
                        {
                            alreadyAdded = true;
                            break;
                        }
                    }

                    if (!alreadyAdded)
                    {
                        targets.Add(childDelegate);
                    }
                }

                for (int i = 0; i < targets.Count; i++)
                {
                    if (targets[i] == null)
                    {
                        targets[i] = null;
                    }
                }
            }
        }

        public override void Perform(GameObject sender) 
        {
            AddAction(new DelayedAction(executionTime, sender));  
        }

        public override void Perform(GameObject sender, bool value) 
        {
            AddAction(new DelayedBoolAction(executionTime, sender, value));
        }

        public override void Perform(GameObject sender, float value) 
        {
            AddAction(new DelayedFloatAction(executionTime, sender, value));
        }

        public override void Perform(GameObject sender, Vector2 value) 
        {
            AddAction(new DelayedVector2Action(executionTime, sender, value));
        }

        public override void Perform(GameObject sender, Vector3 value) 
        {
            AddAction(new DelayedVector3Action(executionTime, sender, value));
        }

        public override void Perform(GameObject sender, GameObject target) 
        {
            AddAction(new DelayedGameObjectAction(executionTime, sender, target));
        }
    }
}

