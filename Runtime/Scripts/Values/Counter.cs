/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;

namespace PuzzleBox
{
    public class Counter : PuzzleBoxBehaviour
    {
        public ObservableInt count = new ObservableInt();
        public int startCount = 0;
        public int minimumValue = 0;
        public int maximumValue = int.MaxValue;
        public int increment = 1;
        public bool cycle = false;

        [Space]
        public NumberValue referencedValue = null;

        [Space]
        public ActionDelegate[] reachedMinimumActions;
        public ActionDelegate[] reachedMaximumActions;

        private void Start()
        {
            //SetCount(startCount);
        }

        protected void OnEnable()
        {
            if (referencedValue != null)
            {
                count.Set((int)referencedValue.value);
            }
        }

        private void UpdateCount(int change)
        {
            if (isActiveAndEnabled)
            {
                if (count == maximumValue && change > 0)
                {
                    if (cycle)
                    {
                        SetCount(minimumValue - 1);
                    }
                    else
                    {
                        return;
                    }
                }

                if (count == minimumValue && change < 0)
                {
                    if (cycle)
                    {
                        SetCount(maximumValue + 1);
                    }
                    else
                    {
                        return;
                    }
                }

                SetCount(count + change);

                if (count >= maximumValue)
                {
                    SetCount(maximumValue);
                    foreach (ActionDelegate action in reachedMaximumActions)
                    {
                        if (action != null)
                        {
                            action.Perform(gameObject);
                        }
                    }
                }

                if (count <= minimumValue)
                {
                    SetCount(minimumValue);
                    foreach (ActionDelegate action in reachedMinimumActions)
                    {
                        if (action != null)
                        {
                            action.Perform(gameObject);
                        }
                    }
                }
            }
        }

        public void SetCount(int newCount)
        {
            if (referencedValue != null)
            {
                referencedValue.Set(newCount);
            }

            if (count != newCount)
            {
                count.Set(newCount);
            }
        }

        [PuzzleBox.Action]
        public void Increment()
        {
            UpdateCount(increment);
        }

        [PuzzleBox.Action]
        public void Decrement()
        {
            UpdateCount(-increment);
        }

        [PuzzleBox.Action]
        public void Reset()
        {
            SetCount(startCount);
        }

        public override string GetIcon()
        {
            return "CounterIcon";
        }

        public override string ToString()
        {
            return count.ToString();
        }
    }
}

