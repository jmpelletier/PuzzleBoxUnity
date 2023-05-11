using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PuzzleBox
{
    public class ResultLabel : MonoBehaviour
    {
        public TextMeshProUGUI label;

        // Start is called before the first frame update
        void Start()
        {
            if (label == null)
            {
                label = GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        public void SetText(string text)
        {
            if (label != null)
            {
                label.text = text;
            }
        }

        public static void Show(GameObject prefab, string message, Vector3 position)
        {
            GameObject result = Instantiate(prefab);
            ResultLabel resultLabel = result.GetComponent<ResultLabel>();
            resultLabel.SetText(message);
            result.transform.position = position;
        }
    }
}

