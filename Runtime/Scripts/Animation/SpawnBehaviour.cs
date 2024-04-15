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
    public class SpawnBehaviour : StateMachineBehaviour
    {
        public GameObject prefab;
        public bool destroyOnExit = true;
        public bool parentInstance = true;
        public Vector3 position = Vector3.zero;

        [Header("Particles")]
        public bool playParticlesOnEnter = true;
        public bool stopParticlesOnExit = true;

        GameObject instance = null;
        ParticleSystem particleSystem = null;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (prefab)
            {
                instance = Instantiate(prefab);
                if (parentInstance)
                {
                    instance.transform.parent = animator.transform;
                }
                instance.transform.localPosition = position;

                particleSystem = instance.GetComponentInChildren<ParticleSystem>();
            }

            if (particleSystem && playParticlesOnEnter)
            {
                particleSystem.Play();
            }
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (particleSystem && stopParticlesOnExit)
            {
                particleSystem.Stop();
            }

            if (instance && destroyOnExit)
            {
                Destroy(instance);
                instance = null;
            }
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }
    }
}

