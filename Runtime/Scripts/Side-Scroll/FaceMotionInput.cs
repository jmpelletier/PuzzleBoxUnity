using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    [AddComponentMenu("Puzzle Box/Side-Scroll/Face Motion Input")]
    public class FaceMotionInput : MonoBehaviour
    {
        public bool flip = true;
        public float minAngle = -90f;
        public float maxAngle = 90f;
        public float threshold = 0.1f;

        MoveBehavior moveBehavior;
        float scaleY = 1;

        // Start is called before the first frame update
        void Start()
        {
            moveBehavior = GetComponentInParent<MoveBehavior>();
            if (moveBehavior == null)
            {
                Debug.LogWarning("MoveBehaviorが見つかりません。親オブジェクトにMoveBehaviorがないとこのコンポーネントが作動しません。");
            }

            scaleY = transform.localScale.y;
        }

        // Update is called once per frame
        void Update()
        {
            if (moveBehavior != null)
            {
                if (moveBehavior.motionInput.magnitude > threshold)
                {
                    float angle = Mathf.Atan2(moveBehavior.motionInput.y, moveBehavior.motionInput.x) * Mathf.Rad2Deg;
                    
                    if (angle > 90f)
                    {
                        angle = 180f - Mathf.Clamp(180f - angle, minAngle, maxAngle);
                    }
                    else if (angle < -90f)
                    {
                        angle = -180f - Mathf.Clamp(-180f - angle, minAngle, maxAngle);
                    }
                    else
                    {
                        angle = Mathf.Clamp(angle, minAngle, maxAngle);
                    }
                    
                    transform.localEulerAngles = new Vector3(0, 0, angle);

                    if (flip && Mathf.Abs(moveBehavior.motionInput.x) > threshold)
                    {
                        Vector3 scale = transform.localScale;

                        scale.y = Mathf.Sign(moveBehavior.motionInput.x) * scaleY;

                        transform.localScale = scale;
                    }
                }
            }
        }
    }
} // namespace

