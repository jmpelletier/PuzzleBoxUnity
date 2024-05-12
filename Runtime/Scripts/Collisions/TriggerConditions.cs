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

        public GameObject[] contactTriggers;
        public GameObject[] noContactTriggers;

        public ActionDelegate[] trueActions;
        public ActionDelegate[] falseActions;

        public int contactCount = 0;
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
                parent?.EnteredTrigger(this);
            }

            private void OnTriggerExit2D(Collider2D collision)
            {
                parent?.ExitedTrigger(this);
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

        private void PerformActions()
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

        private void EnteredTrigger(ColliderActionListener collision)
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

        private void ExitedTrigger(ColliderActionListener collision)
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

