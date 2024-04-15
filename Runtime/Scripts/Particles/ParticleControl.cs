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
    /**
     * Use this component to indirectly control a ParticleSystem
     * from an AnimationClip.
     */

    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleControl : MonoBehaviour
    {
        ParticleSystem _system;

        private ParticleSystem system
        {
            get
            {
                if (!_system)
                {
                    _system = GetComponent<ParticleSystem>();
                }
                return _system;
            }
        }

        private void OnEnable()
        {
            system.Play();
        }

        private void OnDisable()
        {
            system.Stop();
        }

        public void Play()
        {
            system.Play();
        }

        public void Stop()
        {
            system.Stop();
        }

        public void Pause()
        {
            system.Pause();
        }
    }

}
