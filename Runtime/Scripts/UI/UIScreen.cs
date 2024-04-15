/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace PuzzleBox
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent (typeof(CanvasGroup))]
    public class UIScreen : PuzzleBoxBehaviour
    {
        public Selectable defaultSelection;
        public bool exclusiveInteraction = true;

        Animator animationController;
        CanvasGroup canvasGroup;

        public static List<UIScreen> instances = new List<UIScreen>();

        public bool interactable
        {
            get
            {
                return canvasGroup.interactable;
            }

            set 
            { 
                canvasGroup.interactable = value;

                if (canvasGroup.interactable) 
                {
                    DefaultSelection();

                    if (exclusiveInteraction)
                    {
                        foreach (UIScreen screen in instances)
                        {
                            if (screen != this)
                            {
                                screen.interactable = false;
                            }
                        }
                    }
                }
            }
        }

        private void DefaultSelection()
        {
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject != gameObject)
            {
                EventSystem.current.currentSelectedGameObject.SendMessage("OnAutomaticSelection", defaultSelection.gameObject, SendMessageOptions.DontRequireReceiver);
                defaultSelection?.Select();
            }
            
        }

        void Awake()
        {
            animationController = GetComponent<Animator>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            Animator[] animators = GetComponentsInChildren<Animator>();
            foreach (Animator animator in animators)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
        }

        private void OnEnable()
        {
            instances.Add(this);
        }

        private void OnDisable()
        {
            instances.Remove(this);
        }

        public void Show(bool animated = true)
        {
            gameObject.SetActive(true);
            

            if (animated)
            {
                animationController.SetTrigger("Show");
            }
            else
            {
                ShowTransitionComplete();
            }
        }

        public void Hide(bool animated = true)
        {
            if (animated)
            {
                animationController.SetTrigger("Hide");
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void ShowTransitionComplete()
        {
            interactable = true;
            DefaultSelection();
        }

        public void HideTransitionComplete()
        {

        }
    }
}

