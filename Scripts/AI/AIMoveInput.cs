using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    public class AIMoveInput : MonoBehaviour
    {
        public Vector2 input = Vector2.zero;

        private void FixedUpdate()
        {
            SendMessage("Move", input);
        }
    }
}

