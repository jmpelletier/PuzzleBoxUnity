/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEngine;

namespace PuzzleBox
{
    public class Counter : ValueBase
    {
        public int count = 0;
        public int minimumValue = 0;
        public int maximumValue = 10;
        public int increment = 1;
        public bool cycle = false;

        [Space]
        public NumberValue referencedValue = null;

        [Space]
        public ActionDelegate[] reachedMinimumActions;
        public ActionDelegate[] reachedMaximumActions;

        private void Start()
        {
            SetCount(count);
        }

        private void OnEnable()
        {
            if (referencedValue != null)
            {
                count = (int)referencedValue.value;
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
                    count = maximumValue;
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
                    count = minimumValue;
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

            count = newCount;

            OnValueChanged?.Invoke();
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
            SetCount(minimumValue);
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

