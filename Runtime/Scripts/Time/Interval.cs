/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{
    [AddComponentMenu("Puzzle Box/Time/Interval")]
    public class Interval : TimerBase
    {
        public float interval = 1f;
        public float randomVariation = 0f;

        public ActionDelegate[] OnTick;

        float timeLeft = 0f;

        public override void StartTimer()
        {
            base.StartTimer();

            time = 0;
            ResetTimeLeft();
        }

        void ResetTimeLeft()
        {
            float minTime = Mathf.Max(0, interval - randomVariation);
            float maxTime = Mathf.Max(minTime, interval + randomVariation);

            timeLeft = UnityEngine.Random.Range(minTime, maxTime);
        }

        protected override void UpdateTime(float deltaTime)
        {
            if (GetToggleState())
            {
                timeLeft -= deltaTime;
                if (timeLeft <= 0)
                {
                    timeLeft = 0;
                    ActionDelegate.Invoke(OnTick, gameObject);
                    ResetTimeLeft();
                }

                ActionDelegate.Invoke(OnUpdate, gameObject);
                ActionDelegate.Invoke(OnUpdate, gameObject, timeLeft);
            }
        }
    }
}

