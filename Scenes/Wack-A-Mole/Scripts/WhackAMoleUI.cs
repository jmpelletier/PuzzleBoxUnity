using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PuzzleBox
{
    public class WhackAMoleUI : MonoBehaviour
    {
        public CanvasTransition startScreen;
        public CanvasTransition resultScreen;
        public CanvasTransition hud;
        public TextMeshProUGUI scoreLabel;
        public TextMeshProUGUI finalScoreLabel;
        public TextMeshProUGUI timeLabel;

        public void ShowStartScreen()
        {
            startScreen.Show();
            resultScreen.Hide();
            hud.Hide();
        }

        public void ShowPlayScreen()
        {
            startScreen.Hide();
            resultScreen.Hide();
            hud.Show();
        }

        public void ShowResultScreen()
        {
            startScreen.Hide();
            resultScreen.Show();
            hud.Hide();
        }

        public void SetScore(int score)
        {
            scoreLabel.text = $"{score}";
            finalScoreLabel.text = $"{score}";
        }

        public void SetTime(float time)
        {
            timeLabel.text = $"{time:0.0}";
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
