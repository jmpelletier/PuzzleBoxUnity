using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleBox
{
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Image))]
    public class CanvasTransition : MonoBehaviour
    {
        public float inTransitionTime = 0.2f;
        public float outTransitionTime = 0.2f;
        public float maskScale = 1f;

        Animator animationController;

        Image mask;
        Material maskMaterial;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        private void OnEnable()
        {
            animationController = GetComponent<Animator>();
        }

        private void LateUpdate()
        {
            if (mask == null)
            {
                mask = GetComponent<Image>();
            }

            if (mask != null && mask.IsActive())
            {
                float currentScale = mask.material.GetFloat("_Scale");
                if (currentScale != maskScale)
                {
                    mask.materialForRendering.SetFloat("_Scale", maskScale);
                }
            }
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
