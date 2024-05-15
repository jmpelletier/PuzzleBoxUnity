/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{
    [AddComponentMenu("Puzzle Box/Time/Timer")]
    public class Timer : PuzzleBoxBehaviour
    {
        public enum CountDirection
        {
            Up,
            Down
        }

        public float duration = 10f;
        public CountDirection countDirection = CountDirection.Down;
        public bool repeat = false;

        public ActionDelegate[] OnStart;
        public ActionDelegate[] OnUpdate;
        public ActionDelegate[] OnPause;
        public ActionDelegate[] OnEnd;

        public bool useFixedUpdate = true;

        public float time { get; protected set; }

        private void Start()
        {
            if (GetToggleState())
            {
                StartTimer();
            }
        }

        [PuzzleBox.Action]
        public void StartTimer()
        {
            Toggle(true);
            ResetTimer();
            ActionDelegate.Invoke(OnStart, gameObject);
        }

        [PuzzleBox.Action]
        public virtual void PauseTimer()
        {
            if (GetToggleState() == true)
            {
                Toggle(false);
                ActionDelegate.Invoke(OnPause, gameObject);
            }
        }

        [PuzzleBox.Action]
        public void ResetTimer()
        {
            if (countDirection == CountDirection.Down)
            {
                time = duration;
            }
            else
            {
                time = 0;
            }
        }

        protected void UpdateTime(float deltaTime)
        {
            if (GetToggleState())
            {
                if (countDirection == CountDirection.Down)
                {
                    time -= deltaTime;
                    if (time <= 0)
                    {
                        time = duration;
                        Toggle(repeat);
                        ActionDelegate.Invoke(OnEnd, gameObject);
                        return;
                    }
                }
                else
                {
                    time += deltaTime;
                    if (time >= duration)
                    {
                        time = 0;
                        Toggle(repeat);
                        ActionDelegate.Invoke(OnEnd, gameObject);
                        return;
                    }
                }

                ActionDelegate.Invoke(OnUpdate, gameObject);
                ActionDelegate.Invoke(OnUpdate, gameObject, time);
            }

        }

        protected override void PerformUpdate(float deltaSeconds)
        {
            if (!useFixedUpdate)
            {
                UpdateTime(Time.deltaTime);
            }
        }

        protected override void PerformFixedUpdate(float deltaSeconds)
        {
            if (useFixedUpdate)
            {
                UpdateTime(Time.fixedDeltaTime);
            }
        }

        public override string GetIcon()
        {
            return "TimerIcon";
        }
    }
}

