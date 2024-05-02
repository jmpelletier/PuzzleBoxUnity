/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Reflection;
using System.Linq;

namespace PuzzleBox
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class MethodAttribute : Attribute
    {
        public MethodAttribute() { }

        public static MethodInfo[] GetMethods<T>(Type type) where T : MethodAttribute
        {
            return type.GetMethods()
                .Where(x => x.GetCustomAttributes<T>().Any())
                .ToArray();
        }


        private static readonly MethodInfo[] noMethods = new MethodInfo[0];
        public static MethodInfo[] GetMethods<T>(PuzzleBoxBehaviour behaviour) where T : MethodAttribute
        {
            if (behaviour != null)
            {
                return GetMethods<T>(behaviour.GetType());
            }
            else
            {
                return noMethods;
            }
        }
    }
}
