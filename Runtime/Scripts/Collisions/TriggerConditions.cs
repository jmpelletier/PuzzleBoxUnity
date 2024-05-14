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
    public class TriggerConditions2D : PuzzleBoxBehaviour
    {
        [Space]
        public GameObject[] contactTriggers;
        public GameObject[] noContactTriggers;

        [Space]
        public ActionDelegate[] trueActions;
        public ActionDelegate[] falseActions;

        [Space]
        public string targetTag = string.Empty;
        public string ignoreTag = string.Empty;
        public LayerMask layerMask = ~0;

        [System.NonSerialized]
        public int contactCount = 0;

        [System.NonSerialized]
        public int noContactCount = 0;

        bool conditionStatus = false;

        private enum ContactMode
        {
            Contact,
            NoContact
        }

        private class ColliderActionListener : MonoBehaviour
        {
            public TriggerConditions2D parent;
            public ContactMode contactMode;

            private void OnTriggerEnter2D(Collider2D collision)
            {
                if (parent != null && parent.ShouldProcessCollision(collision))
                {
                    parent.EnteredTrigger(this, collision);
                }
                
            }

            private void OnTriggerExit2D(Collider2D collision)
            {
                if (parent != null && parent.ShouldProcessCollision(collision))
                {
                    parent.ExitedTrigger(this, collision);
                }
            }

            Collider2D[] contacts = new Collider2D[8];

            public void CheckForContacts()
            {
                Collider2D[] colliders = GetComponents<Collider2D>();
                
                foreach(Collider2D col in colliders)
                {
                    int count = Physics2D.OverlapCollider(col, default, contacts);
                    for (int i = 0; i < count; i++)
                    {
                        if (!contacts[i].isTrigger)
                        {
                            OnTriggerEnter2D(contacts[i]);
                        }
                    }
                }
            }
        }

        public bool ShouldProcessCollision(Collider2D collision)
        {
            return collision != null &&
                !collision.isTrigger &&
                (string.IsNullOrWhiteSpace(targetTag) || collision.CompareTag(targetTag)) &&
                (string.IsNullOrWhiteSpace(ignoreTag) || !collision.CompareTag(ignoreTag)) &&
                (layerMask == (layerMask | 1 << collision.gameObject.layer));
        }

        void ListenToColliders(GameObject[] objects, ContactMode mode)
        {
            foreach (GameObject obj in objects)
            {
                if (obj != null)
                {
                    ColliderActionListener listener = obj.AddComponent<ColliderActionListener>();
                    listener.parent = this;
                    listener.contactMode = mode;
                    listener.CheckForContacts();
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            ListenToColliders(contactTriggers, ContactMode.Contact);
            ListenToColliders(noContactTriggers, ContactMode.NoContact);

            PerformActions();
        }

        [PuzzleBox.Action]
        public override void Enable(GameObject sender = null)
        {
            base.Enable(sender);

            PerformActions();
        }

        [PuzzleBox.Action]
        public override void Disable(GameObject sender = null)
        {
            base.Disable(sender);
        }

        IEnumerator DoPerformActions()
        {
            yield return new WaitForFixedUpdate();

            if (isActiveAndEnabled)
            {
                if (conditionStatus && trueActions != null)
                {
                    foreach (ActionDelegate action in trueActions)
                    {
                        action?.Perform(gameObject);
                    }
                }
                else if (falseActions != null)
                {
                    foreach (ActionDelegate action in falseActions)
                    {
                        action?.Perform(gameObject);
                    }
                }
            }
        }

        private void PerformActions()
        {
            if (conditionStatus)
            {
                StartCoroutine(DoPerformActions());
            }
        }

        private bool CheckCount()
        {
            bool status = noContactCount == 0 && contactCount > 0;
            if (status != conditionStatus)
            {
                conditionStatus = status;

                PerformActions();
            }

            return status;
        }

        private void EnteredTrigger(ColliderActionListener collision, Collider2D collider)
        {
            if (collision.contactMode == ContactMode.Contact)
            {
                contactCount++;
            }
            else
            {
                noContactCount++;
            }

            CheckCount();
        }

        private void ExitedTrigger(ColliderActionListener collision, Collider2D collider)
        {
            if (collision.contactMode == ContactMode.Contact)
            {
                contactCount--;
            }
            else
            {
                noContactCount--;
            }

            CheckCount();
        }
    }
}

