/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;

namespace PuzzleBox
{
    /**
     * The DynamicDictionary is a wrapper over Dictionary<string, object> that allow storing
     * values of arbitrary types.
     * Callbacks can also be registered for every key to be notified when values are updated.
     */
    public class DynamicDictionary
    {
        private Dictionary<string, object> _dictionary = new Dictionary<string, object>();
        private Dictionary<string, System.Action<object>> _callbacks = new Dictionary<string, System.Action<object>>();

        // Set the value for a key
        public void Set<T>(string key, T value)
        {
            _dictionary[key] = value;
            System.Action<object> callback = _callbacks.GetValueOrDefault(key, null);
            callback?.Invoke(value);
        }

        // Remove a key-pair from the dictionary
        public void Remove(string key)
        {
            _dictionary.Remove(key);
        }

        // Returns true if the key exists
        public bool Contains(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        // Returns a value by the given key, and casts it to the type T.
        // If the key does not exist or if the type does not match
        // the one provided, the default value is used.
        public T Get<T>(string key, T defaultValue = default(T))
        {
            object obj = _dictionary.GetValueOrDefault(key, null);
            if (obj != null && obj.GetType() == typeof(T)) 
            {
                return (T)obj;
            }
            else
            {
                return defaultValue;
            }
        }

        // Register a callback to be called whenever the value for the given
        // key is set. If the key already exists, callback is immediately
        // called once.
        public void Subscribe(string key, System.Action<object> callback)
        {
            if (!_callbacks.ContainsKey(key))
            {
                _callbacks[key] = callback;
            }
            else
            {
                _callbacks[key] += callback;
            }

            if (_dictionary.ContainsKey(key))
            {
                callback?.Invoke(_dictionary[key]);
            }
        }

        // Remove the provided callback.
        public void Unsubscribe(string key, System.Action<object> callback)
        {
            if (_callbacks.ContainsKey(key))
            {
                _callbacks[key] -= callback;
            }
        }
    }
}


