/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using UnityEngine;

namespace PuzzleBox
{
    public class CurveAttribute : PropertyAttribute
    {
        public float startX;
        public float endX;
        public float startY;
        public float endY;
        public Color color;

        public Rect range
        {
            get
            {
                return new Rect(startX, startY, endX - startX, endY - startY);
            }
        }

        public CurveAttribute(float startX, float startY, float endX, float endY, Color color)
        {
            this.startX = startX;
            this.endX = endX;
            this.startY = startY;
            this.endY = endY;
            this.color = color;
        }

        public CurveAttribute(float startX, float startY, float endX, float endY)
        {
            this.startX = startX;
            this.endX = endX;
            this.startY = startY;
            this.endY = endY;
            this.color = Color.green;
        }
    }
}
