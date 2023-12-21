using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleBox
{
    [AddComponentMenu("Puzzle Box/Lifetime")]
    public class Lifetime : MonoBehaviour
    {
        public float lifetime = 10f;
        public bool useAnimator = true;
        public string lifeEndTrigger = "Destroy";
        public UnityEvent OnLifeEnd;

        Animator animationController;

        float timeLeft = 0;

        public float lifeDuration { get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            animationController = GetComponent<Animator>();
            timeLeft = lifetime;
            lifeDuration = 0;

            if (useAnimator && animationController != null)
            {
                bool foundDestroyClip = false;
                foreach(AnimationClip clip in animationController.runtimeAnimatorController.animationClips)
                {
                    if (clip.name == "DestroySelf")
                    {
                        foundDestroyClip = true;
                    }
                }

                if (!foundDestroyClip)
                {
                    Debug.LogError("アニメーションクリップの「DestroySelf」が見つかりません。アニメーターに追加しないとLifetimeがuseAnimatorモードで正しく動作しません。");
                }
            }
        }

        public void SelfDestructNow()
        {
            if (useAnimator && animationController != null)
            {
                animationController.SetTrigger(lifeEndTrigger);
            }

            else
            {
                DestroySelf();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                lifeDuration += Time.deltaTime;

                if (timeLeft <= 0)
                {
                    SelfDestructNow();
                }
            }
        }

        public void DestroySelf()
        {
            OnLifeEnd?.Invoke();
            Destroy(gameObject);
        }
    }
}

