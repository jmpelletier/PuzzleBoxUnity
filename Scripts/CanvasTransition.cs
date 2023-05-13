using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasTransition : MonoBehaviour
    {
        public float inTransitionTime = 0.2f;
        public float outTransitionTime = 0.2f;

        Animator animationController;

        // Start is called before the first frame update
        void Start()
        {
            animationController = GetComponent<Animator>();
        }

        public void Show()
        {
            if (!gameObject.activeSelf)
            {
                Activate();

                if (animationController != null && inTransitionTime > 0f)
                {
                    animationController.speed = 1f / inTransitionTime;
                    animationController.SetTrigger("Show");
                }
            }
            
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                if (animationController != null && outTransitionTime > 0f)
                {
                    animationController.speed = 1f / outTransitionTime;
                    animationController.SetTrigger("Hide");
                }
                else
                {
                    Deactivate();
                }
            }
            
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }

}
