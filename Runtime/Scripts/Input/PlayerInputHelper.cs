/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PuzzleBox
{
    public class InputValue
    {
        private UnityEngine.InputSystem.InputValue _value;
        private InputAction.CallbackContext _context;

        public static implicit operator InputValue(UnityEngine.InputSystem.InputValue value)
        {
            return new InputValue(value);
        }

        public static bool IsPressed(object obj)
        {
            if (obj != null)
            {
                if (obj is InputValue)
                {
                    return ((InputValue)obj).isPressed;
                }
                else if (obj is UnityEngine.InputSystem.InputValue)
                {
                    return ((UnityEngine.InputSystem.InputValue)obj).isPressed;
                }
            }
            return false;
        }

        public static object GetObject(object obj)
        {
            if (obj != null)
            {
                if (obj is InputValue)
                {
                    return ((InputValue)obj).Get();
                }
                else if (obj is UnityEngine.InputSystem.InputValue)
                {
                    return ((UnityEngine.InputSystem.InputValue)obj).Get();
                }
            }
            return null;
        }

        public static TValue GetValue<TValue>(object obj) where TValue : struct
        {
            if (obj != null)
            {
                if (obj is InputValue)
                {
                    return ((InputValue)obj).Get<TValue>();
                }
                else if (obj is UnityEngine.InputSystem.InputValue)
                {
                    return ((UnityEngine.InputSystem.InputValue)obj).Get<TValue>();
                }
            }
            return default(TValue);
        }

        public InputValue(UnityEngine.InputSystem.InputValue value)
        {
            _value = value;
        }

        public InputValue(InputAction.CallbackContext context)
        {
            _context = context;
        }

        public object Get()
        {
            return _value != null ? _value.Get() : (_context.ReadValueAsObject());
        }

        public TValue Get<TValue>() where TValue : struct
        {
            return _value != null ? _value.Get<TValue>() : _context.ReadValue<TValue>();
        }

        public bool isPressed
        {
            get
            {
                return _value != null ? _value.isPressed : _context.ReadValueAsButton();
            }
        }
    }

    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHelper : PuzzleBoxBehaviour
    {
        public enum NotificationMode
        {
            SendMessages,
            BroadcastMessages
        }

        private static Dictionary<Guid, string> messageNames = new Dictionary<Guid, string>();

        public GameObject[] targets;
        public NotificationMode behavior = NotificationMode.SendMessages;

        PlayerInput playerInput;

        void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        }

        private void OnEnable()
        {
            playerInput.onActionTriggered += HandleAction;
        }

        private void OnDisable()
        {
            playerInput.onActionTriggered -= HandleAction;
        }

        private string MakeMethodName(string actionName)
        {
            if (!string.IsNullOrEmpty(actionName))
            {
                if (actionName.Length > 1)
                {
                    return "On" + char.ToUpper(actionName[0]) + actionName.Substring(1);
                }
                else
                {
                    return actionName.ToUpper();
                }
            }
            return string.Empty;
        }


        void HandleAction(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed || (context.canceled && context.action.type == InputActionType.Value))
            {
                InputValue inputValue = new InputValue(context);


                if (!messageNames.ContainsKey(context.action.id))
                {
                    // Capitalize
                    messageNames[context.action.id] = MakeMethodName(context.action.name);
                }

                string messageName = messageNames[context.action.id];

                foreach(GameObject target in targets)
                {
                    if (target != null)
                    {
                        if (behavior == NotificationMode.SendMessages)
                        {
                            target.SendMessage(messageName, inputValue, SendMessageOptions.DontRequireReceiver);
                        }
                        else
                        {
                            target.BroadcastMessage(messageName, inputValue, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }
            }
        }
    }
}

