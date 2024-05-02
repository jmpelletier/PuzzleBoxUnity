/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEditor;

namespace PuzzleBox
{
    // This is a wrapper for loading an asset in the project.
    public class Asset<T> where T : UnityEngine.Object
    {
        public static implicit operator T(Asset<T> asset)
        {
            return asset._asset;
        }

        public static implicit operator Asset<T>(T obj)
        {
            return new Asset<T>(obj);
        }

        public T _asset;
        public Asset(string path)
        {
            try
            {
                _asset = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            catch (System.Exception)
            {
                // File does not exist, set to null
                _asset = null;
            }
        }

        private Asset(T obj)
        {
            _asset = obj;
        }

        public T Get() { return _asset; }
    }
}

