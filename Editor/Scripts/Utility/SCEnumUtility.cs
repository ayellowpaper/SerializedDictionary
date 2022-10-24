using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Editor
{
    internal static class SCEnumUtility
    {
        private static Dictionary<Type, EnumCache> _cache = new Dictionary<Type, EnumCache>();

        internal static EnumCache GetEnumCache(Type enumType)
        {
            if (_cache.TryGetValue(enumType, out var val))
                return val;

            try
            {
                var classType = typeof(EditorGUI).Assembly.GetType("UnityEditor.EnumDataUtility");
                var methodInfo = classType.GetMethod("GetCachedEnumData", BindingFlags.Static | BindingFlags.NonPublic);
                var parameters = new object[] { enumType, true };
                var result = methodInfo.Invoke(null, parameters);
                var flagValues = (int[])result.GetType().GetField("flagValues").GetValue(result);
                var names = (string[])result.GetType().GetField("names").GetValue(result);
                var cache = new EnumCache(flagValues, names);
                _cache.Add(enumType, cache);
                return cache;
            }
            catch
            {
                throw;
            }
        }
    }

    internal record EnumCache
    {
        public readonly int Length;
        public readonly int[] FlagValues;
        public readonly string[] Names;

        private readonly Dictionary<int, string[]> _namesByValue = new Dictionary<int, string[]>();

        public EnumCache(int[] flagValues, string[] displayNames)
        {
            FlagValues = flagValues;
            Names = displayNames;
            Length = flagValues.Length;
        }

        internal string[] GetNamesForValue(int flagValue)
        {
            if (_namesByValue.TryGetValue(flagValue, out var list))
                return list;

            string[] array = Build(flagValue).ToArray();
            _namesByValue.Add(flagValue, array);
            return array;
        }

        private IEnumerable<string> Build(int flagValue)
        {
            if (flagValue == 0)
            {
                yield return FlagValues[0] == 0 ? Names[0] : "Nothing";
                yield break;
            }

            for (int i = 0; i < Length; i++)
            {
                int fv = FlagValues[i];
                if ((fv & flagValue) == fv && fv > 0)
                    yield return Names[i];
            }

            if (FlagValues[Length - 1] != -1 && flagValue == -1)
                yield return "Everything";
        }
    }
}