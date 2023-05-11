using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PuzzleBox
{
    public class Attack2D : MonoBehaviour
    {
        public Action<GameObject> OnAttackObject;
        public Action OnMiss;

        public ContactFilter2D filter;

        Collider2D coll;
        Collider2D[] hits = new Collider2D[8];


        // Start is called before the first frame update
        void Start()
        {
            coll = GetComponent<Collider2D>();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void Attack()
        {
            if (coll != null)
            {
                int overlaps = coll.OverlapCollider(filter, hits);
                for(int i = 0; i < overlaps; i++)
                {
                    OnAttackObject?.Invoke(hits[i].gameObject);
                }

                if (overlaps == 0)
                {
                    OnMiss?.Invoke();
                }
            }
        }
    }
}

