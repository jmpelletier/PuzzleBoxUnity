/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace PuzzleBox
{
    [CustomPropertyDrawer(typeof(SendMessage.MessageTarget))]
    public class SendMessageTargetDrawer : ActionDelegateTargetDrawer
    {
        protected override void DrawProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            base.DrawProperty(position, property, label);
        }
    }
}
