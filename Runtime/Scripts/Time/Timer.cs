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

        public ObservableFloat time = new ObservableFloat();
        public ObservableInt repetitions = new ObservableInt();

        private void Start()
        {
            repetitions.Set(0);
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
            repetitions.Set(0);
            if (countDirection == CountDirection.Down)
            {
                time.Set(duration);
            }
            else
            {
                time.Set(0);
            }
        }

        protected void UpdateTime(float deltaTime)
        {
            if (GetToggleState())
            {
                if (countDirection == CountDirection.Down)
                {
                    time.Set(time -  deltaTime);
                    if (time <= 0)
                    {
                        time.Set(duration);
                        Toggle(repeat);
                        ActionDelegate.Invoke(OnEnd, gameObject);
                        if (repeat)
                        {
                            repetitions.Set(repetitions + 1);
                        }
                        return;
                    }
                }
                else
                {
                    time.Set(time + deltaTime);
                    if (time >= duration)
                    {
                        time.Set(0);
                        Toggle(repeat);
                        ActionDelegate.Invoke(OnEnd, gameObject);
                        if (repeat)
                        {
                            repetitions.Set(repetitions + 1);
                        }
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

