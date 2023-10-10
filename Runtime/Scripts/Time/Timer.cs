using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{
    [AddComponentMenu("Puzzle Box/Time/Timer")]
    public class Timer : Stopwatch
    {
        public enum CountDirection
        {
            Up,
            Down
        }

        [Header("Timer")]
        public float duration = 10f;
        public CountDirection countDirection = CountDirection.Down;

        public UnityEvent OnEnd;

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
            if (active)
            {
                if (countDirection == CountDirection.Down)
                {
                    time -= deltaTime;
                    if (time <= 0)
                    {
                        time = 0;
                        active = false;
                        OnEnd?.Invoke();
                        return;
                    }
                }
                else
                {
                    time += deltaTime;
                    if (time >= duration)
                    {
                        time = duration;
                        active = false;
                        OnEnd?.Invoke();
                        return;
                    }
                }

                OnUpdate?.Invoke(time);
            }
        }
    }
}

