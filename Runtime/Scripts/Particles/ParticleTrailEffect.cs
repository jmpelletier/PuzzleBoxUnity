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
    public class ParticleTrailEffect : ActionDelegate
    {
        public ParticleSystem particles;

        public enum Mode
        {
            Time,
            Distance
        }

        public Mode mode;
        public float maxParticles = 20f;
        public float scale = 0.5f;

        [Curve(0, 0, 1f, 1f)]
        public AnimationCurve countCurve = AnimationCurve.Linear(0, 0, 1, 1);

        // Start is called before the first frame update
        void Awake()
        {
            if (particles == null)
            {
                particles = GetComponent<ParticleSystem>();
            }
        }

        private void Start()
        {
            
        }

        public override void Perform(GameObject sender, float size)
        {
            if (maxParticles > 0f)
            {
                float a = Mathf.Clamp(size * scale / maxParticles, 0, 1);
                float rate = countCurve.Evaluate(a) * maxParticles;
                var emissions = particles.emission;

                PerformAction(() => {
                    if (!particles.isPlaying)
                    {
                        particles.Play();
                    }

                    switch (mode)
                    {
                        case Mode.Time:
                            emissions.rateOverTime = rate;
                            break;
                        case Mode.Distance:
                            emissions.rateOverDistance = rate;
                            break;
                    }
                });
            }
        }

        public override void Pause()
        {
            var emissions = particles.emission;
            emissions.rateOverTime = 0;
            emissions.rateOverDistance = 0;
        }
    }
}

