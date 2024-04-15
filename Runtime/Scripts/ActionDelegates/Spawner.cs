/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEditor;

namespace PuzzleBox
{
    [AddComponentMenu("Puzzle Box/Spawner")]
    public class Spawner : ActionDelegate
    {
        public enum Mode
        {
            Random,
            Sequence
        }

        public GameObject[] prefabs;
        public Mode mode = Mode.Random;

        public Vector3 randomizePosition = Vector2.zero;

        public GameObject parent = null;

        int currentIndex = 0;

        public Action<GameObject> OnSpawn;
        public ActionDelegate[] SpawnActions;

        public void SpawnNow()
        {
            Spawn();
        }

        public GameObject Spawn()
        {
            if (prefabs.Length > 0)
            {
                Vector3 noise = new Vector3(
                    UnityEngine.Random.Range(-randomizePosition.x, randomizePosition.x), 
                    UnityEngine.Random.Range(-randomizePosition.y, randomizePosition.y),
                    UnityEngine.Random.Range(-randomizePosition.z, randomizePosition.z));
                GameObject newObject = Instantiate(prefabs[currentIndex]);
                newObject.transform.position = transform.position + noise;
                newObject.transform.rotation = transform.rotation;
                currentIndex = NextIndex();

                if (parent != null)
                {
                    newObject.transform.parent = parent.transform;
                }

                OnSpawn?.Invoke(newObject);
                ActionDelegate.Invoke(SpawnActions, gameObject);
                ActionDelegate.Invoke(SpawnActions, gameObject, newObject);

                return newObject;
            }

            return null;
        }

        int NextIndex()
        {
            switch (mode)
            {
                case Mode.Random:
                    return UnityEngine.Random.Range(0, prefabs.Length);
                default:
                case Mode.Sequence:
                    return (currentIndex + 1) % prefabs.Length;
            }
        }

        public override void Perform(GameObject sender)
        {
            SpawnNow();
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        private void OnDrawGizmos()
        {
            string items = "";

            if (prefabs != null)
            {
                foreach (var item in prefabs)
                {
                    if (item)
                    {
                        if (!string.IsNullOrEmpty(items))
                        {
                            items += ", ";
                        }
                        items += item.name;
                    }
                }
            }
            
            Handles.Label(transform.position, "Spawn: " + items);
        }

        public override string GetIcon()
        {
            return "SpawnIcon";
        }
    }
}

