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
    public class PlayParticleEffect : ActionDelegate
    {
        public ParticleSystem particles;

        public float maxParticles = 20f;
        public float scale = 0.5f;

        [Curve(0, 0, 1f, 1f)]
        public AnimationCurve burstCountCurve = AnimationCurve.Linear(0, 0, 1, 1);

        // Start is called before the first frame update
        void Awake()
        {
            if (particles == null)
            {
                particles = GetComponent<ParticleSystem>();
            }
        }

        public override void Perform(GameObject sender, float size)
        {
            if (maxParticles > 0f)
            {
                float a = Mathf.Clamp(size * scale / maxParticles, 0, 1);
                int count = (int)Mathf.Floor(burstCountCurve.Evaluate(a) * maxParticles);

                if (delay > 0)
                {
                    PerformAction(() => particles.Emit(count));
                }
                else
                {
                    StartCoroutine(Emit(count));
                }
            }
        }

        private IEnumerator Emit(int count)
        {
            yield return null;
            particles.Emit(count);
        }
    }
}

