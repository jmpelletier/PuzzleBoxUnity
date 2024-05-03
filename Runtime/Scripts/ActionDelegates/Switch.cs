/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{

    public class Switch : ActionDelegate
    {
        public enum Mode
        {
            Toggle,
            Set
        }

        public Mode mode = Mode.Toggle;
        public ActionDelegate.Target[] targets;
        public bool state = true;

        // Start is called before the first frame update
        void Start()
        {

        }

        public override void Perform(GameObject sender, bool value)
        {
            state = value;
            foreach (ActionDelegate.Target target in targets)
            {
                if (target.behaviour != null)
                {
                    PerformAction(() => target.behaviour.Toggle(state));
                }
            }
        }

        public override void Perform(GameObject sender)
        {
            switch(mode)
            {
                case Mode.Toggle:
                    foreach (ActionDelegate.Target target in targets)
                    {
                        if (target.behaviour != null)
                        {
                            PerformAction(() => target.behaviour.Toggle());
                        }
                    }
                    break;
                case Mode.Set:
                    foreach(ActionDelegate.Target target in targets)
                    {
                        if (target.behaviour != null)
                        {
                            PerformAction(() => target.behaviour.Toggle(state));
                        }
                    }
                    break;
            }
        }

        public override string GetIcon()
        {
            return "ToggleIcon";
        }
    }
}

