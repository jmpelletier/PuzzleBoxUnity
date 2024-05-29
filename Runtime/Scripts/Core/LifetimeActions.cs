/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System;
using System.Collections;
using UnityEngine;

namespace PuzzleBox
{
    public class LifetimeActions : PuzzleBoxBehaviour
    {
        public Action<GameObject> OnSpawned;
        public Action<GameObject> OnDestroyed;
        public Action<GameObject> OnKilled;

        protected bool killed = false;

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(SendSpawn());
        }

        IEnumerator SendSpawn()
        {
            yield return null;
            OnSpawned?.Invoke(gameObject);
        }

        protected virtual void WasKilled()
        {
            killed = true;
        }

        private void OnDestroy()
        {
            if (killed)
            {
                OnKilled?.Invoke(gameObject);
            }

            OnDestroyed?.Invoke(gameObject);
        }

    }
}

