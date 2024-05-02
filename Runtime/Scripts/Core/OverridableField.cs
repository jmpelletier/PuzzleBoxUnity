/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using log4net;
using System;
using System.Collections.Generic;

namespace PuzzleBox
{
    [System.Serializable]
    public class OverridableField<T>
    {
        public T defaultValue;

        public const int MAX_OVERRIDES = 8;

        [System.NonSerialized]
        private KeyValuePair<object, T>[] _overrides = new KeyValuePair<object, T>[MAX_OVERRIDES];

        [System.NonSerialized]
        private KeyValuePair<object, T> _currentOverride = default;

        public static implicit operator T(OverridableField<T> o)
        {
            return o != null ? o.Value : default(T);
        }

        public static implicit operator OverridableField<T>(T val)
        {
            return new OverridableField<T>(val);
        }

        public OverridableField()
        {
            defaultValue = default(T);
        }

        public OverridableField(T value)
        {
            this.defaultValue = value;
        }

        public OverridableField(OverridableField<T> otherField)
        {
            if (otherField != null)
            {
                this.defaultValue = otherField.defaultValue;
            }
            else
            {
                this.defaultValue= default(T);
            }
        }

        public T Value {
            get
            {
                if (_currentOverride.Key == null)
                {
                    return defaultValue;
                }
                else
                {
                    return _currentOverride.Value;
                }
            }
        }

        public bool AddOverride(object owner, T value)
        {
            for (int i = 0; i < MAX_OVERRIDES; ++i)
            {
                if (_overrides[i].Key == null)
                {
                    _overrides[i] = new KeyValuePair<object, T>(owner, value);
                    _currentOverride = _overrides[i];
                    return true;
                }
            }

            // We're full...
            return false;
        }

        public void RemoveOverride(object owner)
        {
            for (int i = 0; i < MAX_OVERRIDES; ++i)
            {
                if (_overrides[i].Key == owner)
                {
                    // Update current override
                    if (i > 0)
                    {
                        _currentOverride = _overrides[i - 1];
                    }
                    else
                    {
                        _currentOverride = default;
                    }

                    // Update rest of list
                    int j = i + 1;
                    while(j < MAX_OVERRIDES)
                    {
                        _overrides[i] = _overrides[j];
                        i++;
                        j++;
                    }

                    break;
                }
            }
        }
    }
}
