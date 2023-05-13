using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PuzzleBox
{
    public class RhythmGameUI : MonoBehaviour
    {
        public CanvasTransition startScreen;
        public CanvasTransition resultScreen;
        public CanvasTransition hud;

        public TextMeshProUGUI scoreLabel;
        public TextMeshProUGUI finalScoreLabel;

        public void ShowStartScreen()
        {
            startScreen.Show();
            hud.Hide();
            resultScreen.Hide();
        }

        public void ShowPlayScreen()
        {
            startScreen.Hide();
            hud.Show();
            resultScreen.Hide();
        }

        public void ShowResultScreen()
        {
            startScreen.Hide();
            hud.Hide();
            resultScreen.Show();
        }

        public void SetScore(int score)
        {
            scoreLabel.text = $"{score}";
            finalScoreLabel.text = $"{score}";
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
} // namespace

