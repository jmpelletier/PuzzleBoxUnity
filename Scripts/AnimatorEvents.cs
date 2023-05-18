using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorEvents : MonoBehaviour
    {
        public UnityEvent OnDeactivate;
        public UnityEvent OnActivate;

        Animator animationController;

        float speed = 0f;

        void Awake()
        {
            animationController = GetComponent<Animator>();
        }

        public void StartPlayback()
        {
            Activate();
            animationController.SetTrigger("Start");
        }

        public void Pause()
        {
            if (animationController.speed > 0f)
            {
                speed = animationController.speed;
                animationController.speed = 0f;
            }
        }

        public void Resume()
        {
            if (animationController.speed <= 0f)
            {
                animationController.speed = speed;
            }
        }

        public void Deactivate()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                OnDeactivate?.Invoke();
            }
        }

        public void Activate()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                OnActivate?.Invoke();
            }
        }
    }

}
