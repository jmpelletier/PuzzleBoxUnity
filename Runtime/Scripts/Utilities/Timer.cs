/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System;
using UnityEngine;

namespace PuzzleBox
{
    namespace Utils
    {
        public class Timer
        {
            public bool active = true;
            public float timeLeft { private set; get; }
            public float totalTime { private set; get; }

            public float timeElapsed { get { return totalTime - timeLeft; } }

            public Action OnComplete;
            public Action OnEnd;
            public Action OnStart;
            public Action OnCancel;

            public bool isFinished
            {
                get
                {
                    return timeLeft <= 0;
                }
            }

            public float phase
            {
                get
                {
                    if (totalTime > 0)
                    {
                        return 1f - timeLeft / totalTime;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }


            public void Tick(float dt)
            {
                if (timeLeft > 0 && active)
                {
                    timeLeft -= dt;
                    if (timeLeft <= 0)
                    {
                        timeLeft = 0;
                        OnComplete?.Invoke();
                        OnEnd?.Invoke();
                    }
                }
            }

            public void Start(float time)
            {
                if (time > 0)
                {
                    if (time > timeLeft)
                    {
                        timeLeft = time;
                    }

                    if (time > totalTime)
                    {
                        totalTime = time;
                    }

                    OnStart?.Invoke();
                }
            }

            public void Reset()
            {
                timeLeft = totalTime;
            }

            public void Reset(float time)
            {
                timeLeft = totalTime = time;
            }

            public void Cancel()
            {
                timeLeft = 0;
                OnCancel?.Invoke();
                OnEnd?.Invoke();
            }
        }
    }
}
