/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace PuzzleBox
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ValueLabel : PuzzleBoxBehaviour
    {
        public ValueBase value;
        public string numberFormat = "0.#";

        TextMeshProUGUI label;

        // Start is called before the first frame update
        void Start()
        {
            label = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (value != null)
            {
                value.OnValueChanged += UpdateLabel;
            }
        }

        private void OnDisable()
        {
            if (value != null)
            {
                value.OnValueChanged -= UpdateLabel;
            }
        }

        private void UpdateLabel()
        {
            if (label != null)
            {
                if (value != null)
                {
                    if (value is NumberValue)
                    {
                        label.text = ((NumberValue)value).value.ToString(numberFormat);
                    }
                    else
                    {
                        label.text = value.ToString();
                    }
                }
                else
                {
                    label.text = string.Empty;
                }
            }
        }
    }
}

