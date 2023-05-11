using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    public class WhackAMoleManager : MonoBehaviour
    {
        public float playTime = 10f;

        public Spawner spawner;
        public WhackAMoleUI ui;
        public WhackAMolePlayer player;

        float timeRemaining = 0f;
        bool playing = false;

        public void StartGame()
        {
            ui.ShowPlayScreen();
            player.gameObject.SetActive(true);
            player.score = 0;
            ui.SetScore(0);

            spawner.autoSpawn = true;

            timeRemaining = playTime;
            playing = true;
        }

        public void EndGame()
        {
            ui.ShowResultScreen();
            player.gameObject.SetActive(false);
            spawner.autoSpawn = false;
            playing = false;
        }

        void OnSpawn(GameObject newObject)
        {
            float x = UnityEngine.Random.Range(-5, 5);
            float y = UnityEngine.Random.Range(-3, 3);

            newObject.transform.position = new Vector3(x, y, 0);
        }

        void OnScoreChanged(int newScore)
        {
            ui.SetScore(newScore);
        }

        // Start is called before the first frame update
        void Start()
        {
            ui.ShowStartScreen();
            spawner.autoSpawn = false;
            spawner.OnSpawn += OnSpawn;
            player.OnScoreChanged += OnScoreChanged;
        }

        // Update is called once per frame
        void Update()
        {
            if (playing)
            {
                timeRemaining -= Time.deltaTime;
                if (timeRemaining <= 0)
                {
                    timeRemaining = 0;
                    EndGame();
                }

                ui.SetTime(timeRemaining);
            }
        }
    }
}