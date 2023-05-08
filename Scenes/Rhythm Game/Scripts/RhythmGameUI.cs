using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PuzzleBox
{
    public class RhythmGameUI : MonoBehaviour
    {
        public GameObject startScreen;
        public GameObject resultScreen;
        public GameObject hud;
        public TextMeshProUGUI scoreLabel;
        public TextMeshProUGUI finalScoreLabel;

        public void ShowStartScreen()
        {
            startScreen.SetActive(true);
            hud.SetActive(false);
            resultScreen.SetActive(false);
        }

        public void ShowPlayScreen()
        {
            startScreen.SetActive(false);
            hud.SetActive(true);
            resultScreen.SetActive(false);
        }

        public void ShowResultScreen()
        {
            startScreen.SetActive(false);
            hud.SetActive(false);
            resultScreen.SetActive(true);
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

