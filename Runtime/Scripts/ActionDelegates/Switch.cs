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
        public PuzzleBoxBehaviour[] targets;
        public bool state = true;

        // Start is called before the first frame update
        void Start()
        {

        }

        public override void Perform(GameObject sender, bool value)
        {
            state = value;
            foreach (PuzzleBoxBehaviour target in targets)
            {
                if (target != null)
                {
                    target.Toggle(state);
                }
            }
        }

        public override void Perform(GameObject sender)
        {
            switch(mode)
            {
                case Mode.Toggle:
                    foreach (PuzzleBoxBehaviour target in targets)
                    {
                        if (target != null)
                        {
                            target.Toggle();
                        }
                    }
                    break;
                case Mode.Set:
                    foreach(PuzzleBoxBehaviour target in targets)
                    {
                        if (target != null)
                        {
                            target.Toggle(state);
                        }
                    }
                    break;
            }
        }

        //private void OnDrawGizmosSelected()
        //{
        //    const float lineWidth = 0.1f;
        //    const float arrowheadHalfWidth = 0.2f;
        //    const float arrowheadLength = 0.5f;

        //    foreach (PuzzleBoxBehaviour target in targets)
        //    {
        //        if (target != null)
        //        {
        //            PuzzleBox.EditorUtilities.DrawArrow(transform.position, target.transform.position, lineWidth, arrowheadHalfWidth, arrowheadLength, PuzzleBox.EditorUtilities.redColor);
        //        }
        //    }
        //}

        public override string GetIcon()
        {
            return "ToggleIcon";
        }
    }
}

