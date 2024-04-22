/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;

namespace PuzzleBox
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActionAttribute : Attribute
    {
        public ActionAttribute() { }

        public static MethodInfo[] GetMethods(Type type)
        {
            return type.GetMethods().Where(x => x.GetCustomAttributes<ActionAttribute>().Any()).ToArray();
        }


        private static readonly MethodInfo[] noMethods = new MethodInfo[0];
        public static MethodInfo[] GetMethods(PuzzleBoxBehaviour behaviour)
        {
            if (behaviour != null)
            {
                return GetMethods(behaviour.GetType());
            }
            else
            {
                return noMethods;
            }
        }
    }
}
