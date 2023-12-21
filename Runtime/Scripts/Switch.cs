using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    public class Switch : MonoBehaviour
    {
        public GameObject[] targets;

        private int index = 0;

        // Start is called before the first frame update
        void Start()
        {
            SetIndex(index);
        }

        public void SetIndex(int val)
        {
            if (val >= targets.Length)
            {
                val = targets.Length - 1;
            }

            index = val;

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].SetActive(i == index);
            }
        }
    }
}

