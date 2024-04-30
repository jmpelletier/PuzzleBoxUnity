/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PuzzleBox
{
    public static class Reflection
    {
        public static string[] publishedTypeNames { get; private set; }

        static Reflection()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            publishedTypeNames = assembly.GetTypes()
                .Where(type => IsEnumerable(type))
                .Select(type => type.Name).OrderBy(x => x).ToArray();
        }

        public static bool IsEnumerable(Type type)
        {
            return typeof(PuzzleBoxBehaviour).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !typeof(PuzzleBox.Utility).IsAssignableFrom(type) &&
                    type.GetCustomAttribute<HideInEnumerationAttribute>() == null;
        }
    }
}

