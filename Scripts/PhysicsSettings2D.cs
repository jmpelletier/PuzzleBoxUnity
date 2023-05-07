using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    public class PhysicsSettings2D : MonoBehaviour
    {
        public float gravity = -9.8f;

        void FixedUpdate()
        {
            Physics2D.gravity = Vector2.up * gravity;
        }
    }
} // namespace

