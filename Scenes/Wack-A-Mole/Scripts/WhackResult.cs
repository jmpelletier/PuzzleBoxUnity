using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PuzzleBox
{
    public class WhackResult : MonoBehaviour
    {
        public TextMeshProUGUI label;

        public void SetText(string text)
        {
            label.text = text;
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }

} // namespace
