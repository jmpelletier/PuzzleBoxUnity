using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{
    [AddComponentMenu("Puzzle Box/DestroyZone2D")]
    public class DestroyZone2D : MonoBehaviour
    {
        public string targetTag = "";
        public LayerMask layerMask = ~0;
        public bool destroyOnExit = false;
        public UnityEvent OnDestroy;
        

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!destroyOnExit && (targetTag == "" || collision.tag == targetTag) && (layerMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnDestroy?.Invoke();
                Destroy(collision.gameObject);
            }
            
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (destroyOnExit && (targetTag == "" || collision.tag == targetTag) && (layerMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                OnDestroy?.Invoke();
                Destroy(collision.gameObject);
            }

        }
    }
}

