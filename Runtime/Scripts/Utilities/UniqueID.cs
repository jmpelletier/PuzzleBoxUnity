/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Text.RegularExpressions;

namespace PuzzleBox
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class UniqueID : Utility
    {
        public string uid = "";

#if UNITY_EDITOR
        private static HashSet<string> uniqueIDs = new HashSet<string>();
        private const string uniqueIdNumberRegex = @"(.+) \(([0-9]+)\)$";
#endif

        // Start is called before the first frame update
        void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                string id = uid;
                if (string.IsNullOrWhiteSpace(id) || uniqueIDs.Contains(id))
                {
                    Guid guid;
                    if (string.IsNullOrEmpty(id) || Guid.TryParse(id, out guid))
                    {
                        // Generate a new guid
                        id = Guid.NewGuid().ToString();
                    }
                    else
                    {
                        int i = 1;
                        string new_id = id;
                        do
                        {
                            Match match = Regex.Match(id, uniqueIdNumberRegex);
                            if (match.Success)
                            {
                                int index;
                                if(int.TryParse(match.Groups[2].Value, out index) && index >= 0 && index < int.MaxValue)
                                {
                                    i = index + 1;
                                    new_id = $"{match.Groups[1].Value} ({i})";
                                }
                                else
                                {
                                    new_id = $"{id} ({i})";
                                    i++;
                                }
                            }
                            else
                            {
                                new_id = $"{id} ({i})";
                                i++;
                            }
                            
                        }
                        while (uniqueIDs.Contains(new_id));

                        id = new_id;
                    }

                    uid = id;
                    EditorUtility.SetDirty(this);
                }

                uniqueIDs.Add(uid);
            }
#endif
        }

#if UNITY_EDITOR
        protected override void PerformUpdate(float deltaSeconds)
        {
            if (!Application.isPlaying)
            {
                if (!string.IsNullOrWhiteSpace(uid))
                {
                    uniqueIDs.Add(uid);
                }
                else
                {
                    uid = Guid.NewGuid().ToString();
                }
            }
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                if (uniqueIDs.Contains(uid))
                {
                    uniqueIDs.Remove(uid);
                }
            }
        }
#endif

        public override string ToString()
        {
            return uid;
        }
    }
}

