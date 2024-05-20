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
    public class AnimateStateBehaviour : StateMachineBehaviour
    {
        public Animate.State state;

        Animate animate;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animate == null)
            {
                animate = animator.GetComponent<Animate>();
            }

            if (animate)
            {
                animate.EnteredState(state);
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animate == null)
            {
                animate = animator.GetComponent<Animate>();
            }

            if (animate)
            {
                animate.ExitedState(state);
            }
        }
    }
}


