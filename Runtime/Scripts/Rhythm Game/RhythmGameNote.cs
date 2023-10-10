using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using PuzzleBox;

namespace PuzzleBox
{
    [RequireComponent(typeof(Lifetime))]
    public class RhythmGameNote : MonoBehaviour, IComparable<RhythmGameNote>
    {
        public int type = 0;

        [HideInInspector]
        public float time;

        [HideInInspector]
        public float beat;

        Lifetime lifetime;


        void Start()
        {
            lifetime = GetComponent<Lifetime>();
        }

        public void Clear()
        {
            lifetime.SelfDestructNow();
        }

        public int CompareTo(RhythmGameNote other)
        {
            return (int)Mathf.Sign(time - other.time);
        }

    }
} // namespace

