using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    [ExecuteAlways]
    [RequireComponent(typeof(KinematicMotion2D))]
    public class KinematicPosition2D : MonoBehaviour
    {
        public Vector2 position;
        public float smoothing = 0;


        KinematicMotion2D motion2D;

        // Start is called before the first frame update
        void Start()
        {
            motion2D = GetComponent<KinematicMotion2D>();
        }

        void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                transform.localPosition = position;
            }
#endif
        }

        void FixedUpdate()
        {
            Vector2 delta = position - (Vector2)transform.localPosition;
            Vector2 v = delta / Time.fixedDeltaTime;
            motion2D.velocity = Vector2.Lerp(v, motion2D.velocity, Mathf.Clamp(smoothing, 0, 1));
        }

    }
} // namespace

