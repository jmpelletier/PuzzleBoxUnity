using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PuzzleBox
{
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

        int currentIndex = 0;
        float spawnWaitTime = 0;

        public Action<GameObject> OnSpawn;


        public void Spawn()
        {
            if (prefabs.Length > 0)
            {
                GameObject newObject = Instantiate(prefabs[currentIndex]);
                newObject.transform.position = transform.position;
                newObject.transform.rotation = transform.rotation;
                currentIndex = NextIndex();

                OnSpawn?.Invoke(newObject);
            }
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

