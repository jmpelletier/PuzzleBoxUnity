using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    public class Follow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = Vector3.zero;
        public bool useFixedUpdate = true;
        public bool constrainX = false;
        public bool constrainY = false;
        public bool constrainZ = false;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetTarget(GameObject newTargetObject)
        {
            target = newTargetObject.transform;
        }

        public void ClearTarget()
        {
            target = null;
        }

        void MoveToTarget()
        {
            Vector3 p = transform.position;
            if (!constrainX) p.x = target.position.x + offset.x;
            if (!constrainY) p.y = target.position.y + offset.y;
            if (!constrainZ) p.z = target.position.z + offset.z;
            transform.position = p;
        }

        // Update is called once per frame
        void Update()
        {
            if (!useFixedUpdate && target != null)
            {
                MoveToTarget();
            }
        }

        void FixedUpdate()
        {
            if (useFixedUpdate && target != null)
            {
                MoveToTarget();
            }
        }
    }
}

