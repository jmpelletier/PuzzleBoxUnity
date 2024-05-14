/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using static PuzzleBox.KinematicMotion2D;

namespace PuzzleBox
{
    [RequireComponent(typeof(PlatformerPlayer2D))]
    public class PlatformerPlayer2DActions : LifetimeActions
    {
        PlatformerPlayer2D player;


        [Serializable]
        public struct ContactAction
        {
            public ActionDelegate action;

            [Space]
            public string tag;
            public float minimumSpeed;

            [Space]
            public bool up;
            public bool down;
            public bool left;
            public bool right;

            public void Perform(KinematicMotion2D.Contact contact)
            {
                if (contact != null && action != null)
                {
                    if (tag != "" && !contact.collider.CompareTag(tag))
                    {
                        return;
                    }

                    float speed = contact.relativeVelocity.magnitude;

                    if (speed < minimumSpeed)
                    {
                        return;
                    }

                    if (contact.direction.x < 0 && !left || contact.direction.x > 0 && !right)
                    {
                        return;
                    }

                    if (contact.direction.y < 0 && !down || contact.direction.y > 0 && !up)
                    {
                        return;
                    }

                    action.Perform(contact.self, speed);
                    action.Perform(contact.self, contact.point);
                }
            }
        }

        [Header("Contact")]
        public ContactAction[] contactEnterActions;
        public ContactAction[] contactStayActions;
        public ContactAction[] contactExitActions;

        [Serializable]
        public struct MoveAction
        {
            public ActionDelegate action;

            [Space]
            public float minimumSpeed;
            public Vector2 speedScale;

            [Space]
            public bool walk;
            public bool run;
            public bool jump;
            public bool fall;
            public bool slide;
            public bool climb;
            public bool dash;

            public void Perform(PlatformerPlayer2D player)
            {
                float speed = Vector2.Scale(speedScale, player.velocity).magnitude;
                if (player && action && speed >= minimumSpeed)
                {
                    bool perform = false;

                    switch(player.state)
                    {
                        case PlatformerPlayer2D.State.Walking:
                            perform = walk;
                            break;
                        case PlatformerPlayer2D.State.Running:
                            perform = run;
                            break;
                        case PlatformerPlayer2D.State.Jumping:
                        case PlatformerPlayer2D.State.WallJumping:
                            perform = jump;
                            break;
                        case PlatformerPlayer2D.State.Falling:
                            perform = fall;
                            break;
                        case PlatformerPlayer2D.State.WallSliding:
                            perform = slide;
                            break;
                        case PlatformerPlayer2D.State.ClimbingWallUp:
                        case PlatformerPlayer2D.State.ClimbingWallDown:
                        case PlatformerPlayer2D.State.ClimbingWallOver:
                        case PlatformerPlayer2D.State.Grabbing:
                            perform = climb;
                            break;
                        case PlatformerPlayer2D.State.Dashing:
                            perform = dash;
                            break;
                    }

                    if (perform)
                    {
                        action.Perform(player.gameObject);
                        action.Perform(player.gameObject, speed);
                    }
                    else
                    {
                        action.Pause();
                    }
                }
            }
        }

        [Header("Motion")]
        public MoveAction[] moveActions;

        public ActionDelegate[] jumpActions;

        public ActionDelegate[] landActions;

        public ActionDelegate[] dashActions;


        [Serializable]
        public struct TimerAction
        {
            public ActionDelegate action;

            
            public enum Mode
            {
                Phase,
                Time
            }

            [Space]
            public Mode mode;

            public enum Direction
            {
                Up,
                Down
            }
            public Direction direction;

            public void Perform(PlatformerPlayer2D player, Utils.Timer timer)
            {
                if (action != null)
                {
                    if (!timer.active || timer.isFinished)
                    {
                        action.Pause();
                    }
                    else
                    {
                        if (mode == Mode.Time)
                        {
                            if (direction == Direction.Up)
                            {
                                action.Perform(player.gameObject, timer.timeElapsed);
                            }
                            else
                            {
                                action.Perform(player.gameObject, timer.timeLeft);
                            }
                            
                        }
                        else
                        {
                            if (direction == Direction.Up)
                            {
                                action.Perform(player.gameObject, timer.phase);
                            }
                            else
                            {
                                action.Perform(player.gameObject, 1f - timer.phase);
                            }
                        }
                    }
                }
            }
        }

        void DidJump()
        {
            foreach(ActionDelegate action in jumpActions)
            {
                action?.Perform(player.gameObject);
            }
        }

        void DidLand()
        {
            foreach (ActionDelegate action in landActions)
            {
                action?.Perform(player.gameObject);
            }
        }

        void DidDash()
        {
            foreach (ActionDelegate action in dashActions)
            {
                action?.Perform(player.gameObject);
            }
        }

        [Header("Timers")]
        public TimerAction[] dashCoolDownActions;
        public TimerAction[] wallGrabActions;

        [Header("Lifetime")]
        public ActionDelegate[] spawnActions;
        public ActionDelegate[] dieActions;

        protected override void WasKilled()
        {
            base.WasKilled();

            foreach (ActionDelegate action in dieActions)
            {
                action?.Perform(player.gameObject);
            }
        }

        private void Awake()
        {
            player = GetComponent<PlatformerPlayer2D>();
        }

        private void Start()
        {
            foreach(ActionDelegate action in spawnActions)
            {
                action?.Perform(gameObject);
            }
        }

        void OnContactEnter(KinematicMotion2D.Contact contact)
        {
            foreach(ContactAction action in contactEnterActions)
            {
                action.Perform(contact);
            }
        }

        void OnContactExit(KinematicMotion2D.Contact contact)
        {
            foreach (ContactAction action in contactExitActions)
            {
                action.Perform(contact);
            }
        }

        void OnContactStay(KinematicMotion2D.Contact contact)
        {
            foreach (ContactAction action in contactStayActions)
            {
                action.Perform(contact);
            }
        }

        private void Update()
        {
            foreach (MoveAction action in moveActions)
            {
                action.Perform(player);
            }

            foreach (TimerAction action in dashCoolDownActions)
            {
                action.Perform(player, player.dashCoolDownTimer);
            }

            foreach (TimerAction action in wallGrabActions)
            {
                action.Perform(player, player.wallGrabTimer);
            }
        }
    }
}
