using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{
    [AddComponentMenu("Puzzle Box/Time/Stopwatch")]
    public class Stopwatch : MonoBehaviour
    {

        [Header("Stopwatch")]
        public bool active = false;
        public bool useFixedUpdate = false;

        public UnityEvent OnStart;
        public UnityEvent<float> OnUpdate;
        public UnityEvent<bool> OnPause;

        public float time { get; protected set; }

        private void Start()
        {
            if (active)
            {
                StartTimer();
            }
        }

        public virtual void StartTimer()
        {
            active = true;
            time = 0;
            OnStart?.Invoke();
        }

        public void PauseTimer(bool paused)
        {
            if (active == paused)
            {
                active = !paused;
                OnPause?.Invoke(paused);
            }
        }

        protected virtual void UpdateTime(float deltaTime)
        {
            if (active)
            {
                time += deltaTime;
                OnUpdate?.Invoke(time);
            }
        }

        private void Update()
        {
            if (!useFixedUpdate)
            {
                UpdateTime(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (useFixedUpdate)
            {
                UpdateTime(Time.fixedDeltaTime);
            }
        }
    }
}

