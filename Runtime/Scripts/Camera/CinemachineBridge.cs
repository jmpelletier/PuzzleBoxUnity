/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Cinemachine;

namespace PuzzleBox
{
    public class CinemachineBridge : MonoBehaviour
    {
        //CinemachineVirtualCamera vcam;

        // Start is called before the first frame update
        void Awake()
        {
            //vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        }

        public void Follow(GameObject target)
        {
            //vcam.Follow = target.transform;
        }
    }
}

