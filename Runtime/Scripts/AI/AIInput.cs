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
    public class AIInput : PuzzleBoxBehaviour
    {
        public enum NotificationMode
        {
            SendMessages,
            BroadcastMessages
        }

        public enum HorizontalDirection
        {
            Left,
            Right
        }

        public GameObject[] targets;
        public NotificationMode behavior = NotificationMode.SendMessages;

        [Header("Move")]
        public HorizontalDirection moveDirection = HorizontalDirection.Right;
        public bool isMoving = false;

        [Header("Jump")]
        [Min(0)]
        public float jumpButtonPressTime = 1f;
        [Min(0)]
        public float jumpButtonPressTimeVariation = 0f;

        Vector2 moveInput = Vector2.right;
        float moveMagnitude = 0f;

        Utils.Timer jumpTimer = new Utils.Timer();

        void Send(string message, PuzzleBox.InputValue inputValue = null)
        {
            foreach (GameObject target in targets)
            {
                if (target != null)
                {
                    if (behavior == NotificationMode.SendMessages)
                    {
                        target.SendMessage(message, inputValue, SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        target.BroadcastMessage(message, inputValue, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        private void Start()
        {
            jumpTimer.OnEnd += EndJump;
        }

        private void Update()
        {
            jumpTimer.Tick(Time.deltaTime);

            if (isActiveAndEnabled)
            {
                switch (moveDirection)
                {
                    case HorizontalDirection.Left:
                        moveInput = Vector2.left; break;
                    case HorizontalDirection.Right:
                        moveInput = Vector2.right; break;
                }

                moveMagnitude = isMoving ? 1f : 0f;

                PuzzleBox.InputValue moveInputValue = new PuzzleBox.InputValue(moveInput * moveMagnitude);
                Send("OnMove", moveInputValue);
            }
        }

        [PuzzleBox.Action]
        public void Move()
        {
            isMoving = true;
        }

        [PuzzleBox.Action]
        public void Stop()
        {
            isMoving = false;
        }

        [PuzzleBox.Action]
        public void Turn()
        {
            switch (moveDirection)
            {
                case HorizontalDirection.Left:
                    moveDirection = HorizontalDirection.Right; break;
                case HorizontalDirection.Right:
                    moveDirection = HorizontalDirection.Left; break;
            }
        }

        [PuzzleBox.Action]
        public void Jump()
        {
            PuzzleBox.InputValue inputValue = new PuzzleBox.InputValue(true);
            Send("OnJump", inputValue);
            float jumpTime = Mathf.Max(0, Random.Range(jumpButtonPressTime - jumpButtonPressTimeVariation,
                jumpButtonPressTime + jumpButtonPressTimeVariation));
            jumpTimer.Start(jumpTime);
            jumpTimer.active = true;
        }

        [PuzzleBox.Action]
        public void EndJump()
        {
            PuzzleBox.InputValue inputValue = new PuzzleBox.InputValue(false);
            Send("OnJump", inputValue);
            jumpTimer.active = false;
        }

        [PuzzleBox.Action]
        public void Run()
        {
            PuzzleBox.InputValue inputValue = new PuzzleBox.InputValue(true);
            Send("OnRun", inputValue);
        }

        [PuzzleBox.Action]
        public void Walk()
        {
            PuzzleBox.InputValue inputValue = new PuzzleBox.InputValue(false);
            Send("OnRun", inputValue);
        }

        [PuzzleBox.Action]
        public void Dash()
        {
            PuzzleBox.InputValue inputValue = new PuzzleBox.InputValue(true);
            Send("OnDash", inputValue);
        }
    }

}
