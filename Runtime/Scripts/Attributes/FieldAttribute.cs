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
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public abstract class FieldAttribute : Attribute
    {
        public FieldAttribute() { }

        public static FieldInfo[] GetFields<T>(Type type) where T : FieldAttribute
        {
            return type.GetFields()
                .Where(x => x.GetCustomAttributes<T>().Any())
                .ToArray();
        }


        private static readonly FieldInfo[] noFields = new FieldInfo[0];
        public static FieldInfo[] GetFields<T>(PuzzleBoxBehaviour behaviour) where T : FieldAttribute
        {
            if (behaviour != null)
            {
                return GetFields<T>(behaviour.GetType());
            }
            else
            {
                return noFields;
            }
        }
    }
}
