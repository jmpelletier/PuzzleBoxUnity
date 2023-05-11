using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PuzzleBox
{
    public class WhackAMoleMole : MonoBehaviour
    {
        public int life = 1;

        public void ApplyDamage(int damage)
        {
            life -= damage;

            if (life <= 0)
            {
                SendMessage("SelfDestructNow", SendMessageOptions.RequireReceiver);
            }
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
