using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{

    [AddComponentMenu("Puzzle Box/Trigger Actions")]
    public class TriggerActions : MonoBehaviour
    {
        public string targetTag = "";
        public LayerMask layerMask = ~0;
        public UnityEvent onEnter;
        public UnityEvent onExit;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((targetTag == "" || collision.tag == targetTag) && (layerMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                onEnter?.Invoke();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if ((targetTag == "" || collision.tag == targetTag) && (layerMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                onExit?.Invoke();
            }
        }
    }
}

