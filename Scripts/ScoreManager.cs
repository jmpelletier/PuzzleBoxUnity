using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{
    public class ScoreManager : MonoBehaviour
    {
        public string valueName = "";
        public float score = 0;
        public UnityEvent<float> OnScoreChanged;
        public UnityEvent<string, float> OnValueChanged;

        public void SetScore(float newScore)
        {
            if (score != newScore)
            {
                score = newScore;
                OnScoreChanged?.Invoke(score);
                OnValueChanged?.Invoke(valueName, score);
            }
        }

        public void ChangeScore(float change)
        {
            if (change != 0)
            {
                score += change;
                OnScoreChanged?.Invoke(score);
                OnValueChanged?.Invoke(valueName, score);
            }
        }
    }
}

