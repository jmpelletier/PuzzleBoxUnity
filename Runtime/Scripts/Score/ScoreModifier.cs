using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace PuzzleBox
{
    public class ScoreModifier : MonoBehaviour
    {
        public Action<float> OnScoreChanged;

        public void ChangeScore(float change)
        {
            if (change != 0)
            {
                OnScoreChanged?.Invoke(change);
            }
        }
    }
}

