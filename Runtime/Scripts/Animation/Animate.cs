/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace PuzzleBox
{
    [RequireComponent(typeof(Animator))]
    [ExecuteAlways]
    public class Animate : ActionDelegate
    {
        public enum State
        {
            On,
            Off,
            Perform
        }

        public enum Mode
        {
            OneShot,
            Toggle
        }
        public Mode mode = Mode.OneShot;
        public bool initialState = true;

        [Header("Actions")]
        public ActionDelegate[] OnTurnOn;
        public ActionDelegate[] OnTurningOff;
        public ActionDelegate[] OnTurnedOff;
        public ActionDelegate[] OnStartPerforming;
        public ActionDelegate[] OnFinishedPerforming;

        private bool state;

        Animator animator;

        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();

            if (animator.runtimeAnimatorController != null)
            {
                animator.SetBool("State", initialState);
            }
                
            state = initialState;
        }

        public void EnteredState(State state)
        {
            switch(state)
            {
                case State.On:
                    ActionDelegate.Invoke(OnTurnOn, gameObject);
                    break;
                case State.Off:
                    ActionDelegate.Invoke(OnTurningOff, gameObject);
                    break;
                case State.Perform:
                    ActionDelegate.Invoke(OnStartPerforming, gameObject);
                    break;
            }
        }

        public void ExitedState(State state)
        {
            switch (state)
            {
                case State.On:
                    break;
                case State.Off:
                    ActionDelegate.Invoke(OnTurnedOff, gameObject);
                    break;
                case State.Perform:
                    ActionDelegate.Invoke(OnFinishedPerforming, gameObject);
                    break;
            }
        }

        public override void Perform(GameObject sender)
        {
            if (animator.runtimeAnimatorController != null)
            {
                if (mode == Mode.Toggle)
                {
                    state = !state;
                    animator.SetBool("State", state);
                }
                else
                {
                    animator.SetTrigger("Perform");
                }
            }
            else
            {
                if (mode == Mode.Toggle)
                {
                    state = !state;
                    if (state)
                    {
                        ExitedState(State.Off);
                        EnteredState(State.On);
                    }
                    else
                    {
                        ExitedState(State.On);
                        EnteredState(State.Off);
                    }
                }
                else
                {
                    EnteredState(State.Perform);
                    ExitedState(State.Perform);
                }
            }
        }

        public override void Perform(GameObject sender, bool newState)
        {
            if (animator.runtimeAnimatorController != null)
            {
                if (mode == Mode.Toggle)
                {
                    state = newState;
                    animator.SetBool("State", state);
                }
                else
                {
                    animator.SetTrigger("Perform");
                }
            }
            else
            {
                if (mode == Mode.Toggle && state != newState)
                {
                    state = newState;
                    if (state)
                    {
                        ExitedState(State.Off);
                        EnteredState(State.On);
                    }
                    else
                    {
                        ExitedState(State.On);
                        EnteredState(State.Off);
                    }
                }
                else
                {
                    EnteredState(State.Perform);
                    ExitedState(State.Perform);
                }
            }
        }
    }

}
