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
    [RequireComponent(typeof(AudioSource))]
    public class UINavigationSounds : MonoBehaviour, 
        IPointerClickHandler, 
        IPointerDownHandler, 
        ISelectHandler, 
        IDeselectHandler,
        IPointerEnterHandler, 
        IPointerExitHandler,
        ISubmitHandler
    {
        public AudioClip pressSound;
        public AudioClip submitSound;
        public AudioClip selectSound;
        public AudioClip enterSound;
        public AudioClip exitSound;

        AudioSource audioSource;

        bool selected = false;
        bool ignoreDeselect = false;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void PlayClip(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                if (audioSource.isActiveAndEnabled)
                {
                    audioSource.PlayOneShot(clip);
                }
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PlayClip(submitSound);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PlayClip(pressSound);
        }

        public void OnSelect(BaseEventData eventData)
        {
            selected = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;

            if (ignoreDeselect)
            {
                ignoreDeselect = false;
                return;
            }

            if (eventData.selectedObject != null)
            {
                PlayClip(selectSound);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PlayClip(enterSound);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PlayClip(exitSound);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            PlayClip(submitSound);
        }

        void OnAutomaticSelection(GameObject newSelection)
        {
            if (selected)
            {
                ignoreDeselect = true;
            }
        }
    }
}

