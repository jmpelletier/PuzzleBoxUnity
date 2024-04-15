/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    namespace Utils
    {
        /**
         * This class allows some values to be remembered for some time.
         * This can be used to implement mechanics such as jump buffering
         * in platformers.
         * 
         * Usage example:
         * 
         * // Buffer for 0.05 seconds. 
         * Utils.BufferedInput<bool> jumpInput = new Utils.BufferedInput<bool>(0.05f);
         * 
         * // Set value
         * bool val = true;
         * jumpInput.Set(val);
         * 
         * // Read value
         * if (jumpInput.HasValue()) {
         *     val = jumpInput.Get();
         * }
         * 
         */
        public class BufferedInput<T>
        {
            public float duration;

            T _value;
            float _time;


            public BufferedInput(float duration)
            {
                this.duration = duration;
                this._time = float.NegativeInfinity;
            }

            public bool HasValue()
            {
                return Time.time - _time <= duration;
            }


            public void Set(T value) 
            { 
                _value = value;
                _time = Time.time;
            }

            public void Set(T value, float duration)
            {
                Set(value);
                this.duration = duration;
            }

            public T Get()
            {
                return _value;
            }

            public void Reset()
            {
                _time = float.NegativeInfinity;
            }

        }

    }
}
