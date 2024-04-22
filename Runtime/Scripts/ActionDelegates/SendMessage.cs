/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PuzzleBox
{
    public class SendMessage : ActionDelegate
    {
        [System.Serializable]
        public struct MessageTarget
        {
            public GameObject target;
            public PuzzleBoxBehaviour behaviour;
        }

        public MessageTarget[] targets;
        public string message = "";

        public override void Perform(GameObject sender)
        {
            foreach(MessageTarget target in targets)
            {
                if (target.behaviour != null)
                {
                    target.behaviour.Invoke(message);
                }
            }
        }
    }
}

