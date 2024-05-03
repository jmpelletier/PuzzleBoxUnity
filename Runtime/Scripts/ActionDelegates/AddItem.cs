using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleBox
{
    public class AddItem : ActionDelegate
    {
        public GameObject item;
        public GameObject inventory;

        Inventory inventoryComponent;
        ObjectReference inventoryReference;

        Item itemComponent;
        ObjectReference itemReference;

        private void Start()
        {
            if (inventory != null)
            {
                inventoryReference = inventory.GetComponent<ObjectReference>();
                inventoryComponent = inventory.GetComponent<Inventory>();
            }

            if (item != null)
            {
                itemComponent = item.GetComponent<Item>();
            }
        }

        private Item GetItemComponent()
        {
            if (itemComponent != null)
            {
                return itemComponent;
            }
            else if (itemReference != null)
            {
                if (itemReference.referencedObject != null)
                {
                    return itemReference.referencedObject.GetComponent<Item>();
                }
            }
            else if (item != null)
            {
                itemComponent = item.GetComponent<Item>();

                if (itemComponent == null)
                {
                    itemReference = item.GetComponent<ObjectReference>();
                    if (itemReference != null)
                    {
                        if (itemReference.referencedObject != null)
                        {
                            return itemReference.referencedObject.GetComponentInChildren<Item>();
                        }
                    }
                }
            }

            return itemComponent;
        }

        public override void Perform(GameObject sender)
        {
            if (inventoryComponent != null)
            {
                Item targetItem = GetItemComponent();

                PerformAction(() => inventoryComponent.GetItem(targetItem));
            }
            else if (inventoryReference != null && inventoryReference.referencedObject != null)
            {
                Inventory targetInventory = inventoryReference.referencedObject.GetComponentInChildren<Inventory>();
                if (targetInventory != null)
                {
                    Item targetItem = GetItemComponent();

                    PerformAction(() => targetInventory.GetItem(targetItem));
                }
            }
        }
    }
}

