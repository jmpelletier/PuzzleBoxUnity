using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasVisibility : MonoBehaviour
    {
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

                if (animationController != null)
                {
                    animationController.SetTrigger("Show");
                }
            }
            
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                if (animationController != null)
                {
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
    }

}
