using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace PuzzleBox
{
    [AddComponentMenu("Puzzle Box/Spawner")]
    public class Spawner : MonoBehaviour
    {
        public enum Mode
        {
            Random,
            Sequence
        }

        public GameObject[] prefabs;
        public Mode mode = Mode.Random;

        public bool autoSpawn = true;
        public float minSpawnTime = 1;
        public float maxSpawnTime = 5;
        public Vector3 randomizePostion = Vector2.zero;

        public GameObject parent = null;

        int currentIndex = 0;
        float spawnWaitTime = 0;

        public Action<GameObject> OnSpawn;
        public UnityEvent<GameObject> SpawnActions;

        public void SpawnNow()
        {
            Spawn();
        }

        public GameObject Spawn()
        {
            if (prefabs.Length > 0)
            {
                Vector3 noise = new Vector3(
                    UnityEngine.Random.Range(-randomizePostion.x, randomizePostion.x), 
                    UnityEngine.Random.Range(-randomizePostion.y, randomizePostion.y),
                    UnityEngine.Random.Range(-randomizePostion.z, randomizePostion.z));
                GameObject newObject = Instantiate(prefabs[currentIndex]);
                newObject.transform.position = transform.position + noise;
                newObject.transform.rotation = transform.rotation;
                currentIndex = NextIndex();

                if (parent != null)
                {
                    newObject.transform.parent = parent.transform;
                }

                OnSpawn?.Invoke(newObject);
                SpawnActions?.Invoke(newObject);

                return newObject;
            }

            return null;
        }

        float NewSpawnWaitTime()
        {
            return UnityEngine.Random.Range(minSpawnTime, maxSpawnTime);
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

        // Start is called before the first frame update
        void Start()
        {
            spawnWaitTime = NewSpawnWaitTime();
        }

        // Update is called once per frame
        void Update()
        {
            if (autoSpawn)
            {
                spawnWaitTime -= Time.deltaTime;
                if (spawnWaitTime <= 0f)
                {
                    Spawn();
                    spawnWaitTime = NewSpawnWaitTime();
                }
            }
        }
    }
}

