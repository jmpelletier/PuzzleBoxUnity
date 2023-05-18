using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{
    public class Metronome : Stopwatch
    {
        [Header("Metronome")]
        public float interval = 1f;
        public float randomVariation = 0f;

        public UnityEvent OnTick;

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
            if (active)
            {
                timeLeft -= deltaTime;
                if (timeLeft <= 0)
                {
                    timeLeft = 0;
                    OnTick?.Invoke();
                    ResetTimeLeft();
                }

                OnUpdate?.Invoke(time);
            }
        }
    }
}

