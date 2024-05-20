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
        public float duration = 1;

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
                animator.speed = duration > 0 ? 1f / duration : 1f/60;
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
                    ActionDelegate.Invoke(OnTurnedOff, gameObject);
                    break;
                case State.Off:
                    break;
                case State.Perform:
                    ActionDelegate.Invoke(OnFinishedPerforming, gameObject);
                    break;
            }
        }

        public void OnLoopStart(State state)
        {
            // Nothing
        }

        public void OnLoopEnd(State state)
        {

        }


        [PuzzleBox.Action]
        public void TurnOn(GameObject sender)
        {
            Perform(sender, true);
        }

        [PuzzleBox.Action]
        public void TurnOff(GameObject sender)
        {
            Perform(sender, false);
        }

        [PuzzleBox.Action]
        public override void Toggle(GameObject sender)
        {
            Perform(sender, !state);
        }

        [PuzzleBox.Action]
        public override void Perform(GameObject sender)
        {
            if (animator.runtimeAnimatorController != null)
            {
                if (mode == Mode.Toggle)
                {
                    state = !state;
                    PerformAction(() => animator.SetBool("State", state));
                }
                else
                {
                    PerformAction(() => animator.SetTrigger("Perform"));
                }
            }
            else
            {
                if (mode == Mode.Toggle)
                {
                    state = !state;
                    if (state)
                    {
                        PerformAction(() => {
                            ExitedState(State.Off);
                            EnteredState(State.On);
                        });
                    }
                    else
                    {
                        PerformAction(() => {
                            ExitedState(State.On);
                            EnteredState(State.Off);
                        });
                    }
                }
                else
                {
                    PerformAction(() => {
                        ExitedState(State.Perform);
                        EnteredState(State.Perform);
                    });
                }
            }
        }

        public override string GetIcon()
        {
            return "AnimateIcon";
        }

        public override void Perform(GameObject sender, bool newState)
        {
            if (animator.runtimeAnimatorController != null)
            {
                if (mode == Mode.Toggle)
                {
                    PerformAction(() => {
                        state = newState;
                        animator.SetBool("State", state);
                    });                    
                }
                else
                {
                    PerformAction(() => {
                        animator.SetTrigger("Perform");
                    });
                }
            }
            else
            {
                if (mode == Mode.Toggle && state != newState)
                {
                    state = newState;
                    PerformAction(() => {
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
                    });
                    
                }
                else
                {
                    PerformAction(() => {
                        EnteredState(State.Perform);
                        ExitedState(State.Perform);
                    });
                }
            }
        }
    }

}
