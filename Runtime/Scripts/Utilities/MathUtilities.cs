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
    public static class MathUtilities
    {
        public static float EaseTowards(float currentValue, float targetValue, float slope, float deltaSeconds)
        {
            float v = currentValue;
            if (targetValue > currentValue)
            {
                v += slope * deltaSeconds;
                if (v > targetValue)
                {
                    v = targetValue;
                }
            }
            else if (targetValue < currentValue)
            {
                v -= slope * deltaSeconds;
                if (v < targetValue)
                {
                    v = targetValue;
                }
            }

            return v;
        }
    }
}

