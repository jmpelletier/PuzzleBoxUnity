/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using UnityEngine;

namespace PuzzleBox
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class ClimbableArea : PuzzleBoxBehaviour
    {
        [Space]
        public string targetTag = string.Empty;
        public string ignoreTag = string.Empty;
        public LayerMask layerMask = ~0;

        [Space]
        public bool canExitTop = true;
        public bool canExitRight = true;
        public bool canExitBottom = true;
        public bool canExitLeft = true;

        [Space]
        public bool snapPositionX = true;
        public bool snapPositionY = false;

        [Space]
        [Min(0)]
        public float ejectForceUp = 0f;
        [Min(0)]
        public float ejectForceDown = 0f;
        [Min(0)]
        public float ejectForceSides = 0f;

        BoxCollider2D boxCollider;


        private void Start()
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }


        bool processTriggerEvent(Collider2D collision)
        {
            return enabled &&
                !collision.isTrigger &&
                (targetTag == "" || collision.tag == targetTag) &&
                (ignoreTag == "" || collision.tag != ignoreTag) &&
                (layerMask.value & (1 << collision.gameObject.layer)) > 0;
        }

        void AdjustPosition(PuzzleBoxBehaviour target)
        {
            PlatformerPlayer2D player = target as PlatformerPlayer2D;
            if (player != null && player.state == PlatformerPlayer2D.State.Climbing)
            {
                Vector2 newPosition = player.rigidbody.position;
                Bounds bounds = player.bounds;

                if (bounds.min.x < boxCollider.bounds.min.x && player.velocity.x < 0 && canExitLeft)
                {
                    player.StopClimbing();
                    return;
                }

                if (bounds.max.x > boxCollider.bounds.max.x && player.velocity.x > 0 && canExitRight)
                {
                    player.StopClimbing();
                    return;
                }

                if (bounds.min.y < boxCollider.bounds.min.y && player.velocity.y < 0 && canExitBottom)
                {
                    player.StopClimbing();
                    return;
                }

                if (bounds.max.y > boxCollider.bounds.max.y && player.velocity.y > 0 && canExitTop)
                {
                    player.StopClimbing();
                    return;
                }

                if (snapPositionX)
                {
                    if (bounds.size.x > boxCollider.bounds.size.x)
                    {
                        newPosition.x = boxCollider.bounds.center.x;
                    }
                    else
                    {
                        if (bounds.min.x < boxCollider.bounds.min.x)
                        {
                            newPosition.x += boxCollider.bounds.min.x - bounds.min.x;
                        }
                        else if (bounds.max.x > boxCollider.bounds.max.x)
                        {
                            newPosition.x += boxCollider.bounds.max.x - bounds.max.x;
                        }
                    }
                }

                if (snapPositionY)
                {
                    if (bounds.size.y > boxCollider.bounds.size.y)
                    {
                        newPosition.y = boxCollider.bounds.center.y;
                    }
                    else
                    {
                        if (bounds.min.y < boxCollider.bounds.min.y)
                        {
                            newPosition.y += boxCollider.bounds.min.y - bounds.min.y;
                        }
                        else if (bounds.max.y > boxCollider.bounds.max.y)
                        {
                            newPosition.y += boxCollider.bounds.max.y - bounds.max.y;
                        }
                    }
                }

                player.rigidbody.position = newPosition;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (processTriggerEvent(collision))
            {
                PlatformerPlayer2D player = collision.GetComponent<PlatformerPlayer2D>();
                if (player != null)
                {
                    player.AddOverride(this, "canClimb", true, 0);
                    player.OnPostFixedUpdateActions += AdjustPosition;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (processTriggerEvent(collision))
            {
                PlatformerPlayer2D player = collision.GetComponent<PlatformerPlayer2D>();
                if (player != null)
                {
                    player.RemoveOverride(this, "canClimb");
                    player.OnPostFixedUpdateActions -= AdjustPosition;

                    if (player.state == PlatformerPlayer2D.State.Climbing)
                    {
                        if (Mathf.Abs(player.velocity.x) > 0.01f && ejectForceSides > 0)
                        {
                            player.velocity.x += Mathf.Sign(player.velocity.x) * ejectForceSides;
                        }

                        if (player.velocity.y > 0.01f && ejectForceUp > 0)
                        {
                            player.velocity.y += ejectForceUp;
                        }
                        else if (player.velocity.y < 0.01f && ejectForceDown > 0)
                        {
                            player.velocity.y -= ejectForceDown;
                        }
                    }
                }
            }
        }

        public override string GetIcon()
        {
            return "CollisionIcon";
        }
    }
}

