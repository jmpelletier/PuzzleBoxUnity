/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using UnityEngine;

namespace PuzzleBox
{
    public class DestroyObject : TargetObject
    {
        public override void Perform(GameObject sender)
        {
            GameObject destroyTarget = target;
            PuzzleBox.ObjectReference reference = target.GetComponent<PuzzleBox.ObjectReference>();
            if (reference != null)
            {
                destroyTarget = reference.referencedObject;
            }

           PerformAction(() => {
               if (destroyTarget != null)
               {
                   Destroy(destroyTarget);
                   destroyTarget.SendMessage("WasDestroyed", null, SendMessageOptions.DontRequireReceiver);
               }
            });
        }
    }
}

