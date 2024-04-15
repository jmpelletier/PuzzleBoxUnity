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
    public abstract class TimerBase : PuzzleBoxBehaviour
    {
        public bool useFixedUpdate = false;

        public ActionDelegate[] OnStart;
        public ActionDelegate[] OnUpdate;
        public ActionDelegate[] OnPause;

        public float time { get; protected set; }

        private void Start()
        {
            if (GetToggleState())
            {
                StartTimer();
            }
        }

        public virtual void StartTimer()
        {
            Toggle(true);
            time = 0;
            ActionDelegate.Invoke(OnStart, gameObject);
        }

        public void PauseTimer(bool paused)
        {
            if (GetToggleState() == paused)
            {
                Toggle(!paused);
                ActionDelegate.Invoke(OnPause, gameObject);
            }
        }

        protected virtual void UpdateTime(float deltaTime)
        {
            if (GetToggleState())
            {
                time += deltaTime;
                ActionDelegate.Invoke(OnUpdate, gameObject);
                ActionDelegate.Invoke(OnUpdate, gameObject, time);
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

