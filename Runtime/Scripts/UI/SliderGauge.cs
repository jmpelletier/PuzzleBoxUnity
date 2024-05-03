/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleBox
{
    public class SliderGauge : ActionDelegate
    {
        public Slider slider;

        public float scale = 1f;

        void Awake()
        {
            if (slider == null)
            {
                slider = GetComponent<Slider>();
            }
        }

        private void Start()
        {
            
        }

        public override void Perform(GameObject sender, float value)
        {
            if (slider != null)
            {
                PerformAction(() => {
                    if (!slider.gameObject.activeSelf)
                    {
                        slider.gameObject.SetActive(true);
                    }
                    slider.value = value * scale;
                });
            }
        }

        public override void Pause()
        {
            slider.value = slider.minValue;
            slider.gameObject.SetActive(false);
        }
    }
}

