using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleBox;

namespace PuzzleBox
{
    public class BaseManager : MonoBehaviour
    {
        public BaseUI ui;
        public BasePlayer player;

        bool playing = false;

        public void StartGame()
        {
            ui.ShowPlayScreen();
            player.gameObject.SetActive(true);
            player.score = 0;
            ui.SetScore(0);

            playing = true;
        }

        public void EndGame()
        {
            ui.ShowResultScreen();
            player.gameObject.SetActive(false);
            playing = false;
        }

        void OnScoreChanged(int newScore)
        {
            ui.SetScore(newScore);
        }

        // Start is called before the first frame update
        void Start()
        {
            ui.ShowStartScreen();
            player.OnScoreChanged += OnScoreChanged;
        }

        // Update is called once per frame
        void Update()
        {
            if (playing)
            {
                
            }
        }
    }
}