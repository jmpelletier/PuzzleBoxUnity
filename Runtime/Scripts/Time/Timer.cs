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
    public class Timer : TimerBase
    {
        public enum CountDirection
        {
            Up,
            Down
        }

        public float duration = 10f;
        public CountDirection countDirection = CountDirection.Down;

        public ActionDelegate[] OnEnd;

        public override void StartTimer()
        {
            base.StartTimer();

            if (countDirection == CountDirection.Down)
            {
                time = duration;
            }
            else
            {
                time = 0;
            }
        }

        protected override void UpdateTime(float deltaTime)
        {
            if (GetToggleState())
            {
                if (countDirection == CountDirection.Down)
                {
                    time -= deltaTime;
                    if (time <= 0)
                    {
                        time = 0;
                        Toggle(false);
                        ActionDelegate.Invoke(OnEnd, gameObject);
                        return;
                    }
                }
                else
                {
                    time += deltaTime;
                    if (time >= duration)
                    {
                        time = duration;
                        Toggle(false);
                        ActionDelegate.Invoke(OnEnd, gameObject);
                        return;
                    }
                }

                ActionDelegate.Invoke(OnUpdate, gameObject);
                ActionDelegate.Invoke(OnUpdate, gameObject, time);
            }
        }
    }
}

