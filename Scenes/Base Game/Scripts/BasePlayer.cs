using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PuzzleBox;

namespace PuzzleBox
{
    public class BasePlayer : MonoBehaviour
    {
        public int score = 0;

        public Action<int> OnScoreChanged;
        

        // Start is called before the first frame update
        void Start()
        {
            UpdateScore(0);
        }

        void UpdateScore(int change)
        {
            score += change;
            OnScoreChanged?.Invoke(score);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
