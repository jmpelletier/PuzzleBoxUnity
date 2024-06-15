/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PuzzleBox
{
    [CustomEditor(typeof(PuzzleBox.ColliderActions))]
    public class ColliderActionsEditor : PuzzleBoxBehaviourEditor
    {
        private void OnSceneGUI()
        {
            ColliderActions colliderActions = (ColliderActions)target;
            Collider2D coll = colliderActions.GetComponent<Collider2D>();
            if (coll != null)
            {
                EditorUtilities.DrawCollider(coll, colliderActions.strokeColor, colliderActions.fillColor);
            }
        }
    }
}
