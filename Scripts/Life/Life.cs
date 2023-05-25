using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PuzzleBox
{
    [RequireComponent(typeof(Lifetime))]
    public class Life : MonoBehaviour
    {
        public float maxLife = 100;
        public float life = 100f;
        public bool canDie = true;

        public Action OnDie;
        public Action<float, float> OnLifeChanged;

        public void ChangeLife(float change)
        {
            life = Mathf.Clamp(life + change, 0, maxLife);

            OnLifeChanged?.Invoke(change, life);

            if (canDie && life <= 0)
            {
                OnDie?.Invoke();
            }

        }

        // Start is called before the first frame update
        void Start()
        {
            Lifetime lifetime = GetComponent<Lifetime>();
            OnDie += lifetime.SelfDestructNow;
        }
    }
}

