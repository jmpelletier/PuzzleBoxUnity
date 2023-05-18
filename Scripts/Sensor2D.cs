using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace PuzzleBox
{
    public class Sensor2D : MonoBehaviour
    {
        public string targetTag = "";
        public LayerMask layerMask = ~0;
        public bool ignoreTriggers = true;

        public UnityEvent<GameObject> OnHit;
        public UnityEvent OnMiss;

        Collider2D coll;

        Collider2D[] hits = new Collider2D[32];
        ContactFilter2D filter = new ContactFilter2D();

        void Awake()
        {
            coll = GetComponent<Collider2D>();
            if (coll == null)
            {
                Debug.LogWarning("コライダーがないのでSensor2Dは動作しません。");
            }
        }

        public void OnScan(InputValue inputValue)
        {
            if (inputValue.isPressed)
            {
                Scan();
            }
        }

        public void Scan()
        {
            if (coll != null)
            {
                filter.layerMask = layerMask;
                filter.useLayerMask = true;
                filter.useTriggers = !ignoreTriggers;
                int count = coll.OverlapCollider(filter, hits);
                bool didHit = false;
                for (int i = 0; i < count; i++)
                {
                    if (targetTag == "" || hits[i].tag == targetTag)
                    {
                        OnHit?.Invoke(hits[i].gameObject);
                        didHit = true;
                    }
                }
                if (!didHit)
                {
                    OnMiss?.Invoke();
                }
            }
        }
    }
}

