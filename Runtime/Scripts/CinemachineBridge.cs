using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace PuzzleBox
{
    public class CinemachineBridge : MonoBehaviour
    {
        CinemachineVirtualCamera vcam;

        // Start is called before the first frame update
        void Awake()
        {
            vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        }

        public void Follow(GameObject target)
        {
            vcam.Follow = target.transform;
        }
    }
}

