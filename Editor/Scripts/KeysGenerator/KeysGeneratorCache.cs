using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Populators
{
    public static class KeysGeneratorCache
    {
        private static List<KeysGeneratorData> _populators;
        private static Dictionary<Type, List<KeysGeneratorData>> _populatorsByType;

        static KeysGeneratorCache()
        {
            _populators = new List<KeysGeneratorData>();
            _populatorsByType = new Dictionary<Type, List<KeysGeneratorData>>();
            var populatorTypes = TypeCache.GetTypesDerivedFrom<KeysGenerator>();
            foreach (var populatorType in populatorTypes.Where(x => !x.IsAbstract))
            {
                var attributes = populatorType.GetCustomAttributes<KeysGeneratorAttribute>();
                foreach (var attribute in attributes)
                    _populators.Add(new KeysGeneratorData(attribute.Name, attribute.TargetType, populatorType, attribute.NeedsWindow));
            }
        }

        public static IReadOnlyList<KeysGeneratorData> GetPopulatorsForType(Type type)
        {
            if (!_populatorsByType.ContainsKey(type))
                _populatorsByType.Add(type, new List<KeysGeneratorData>(_populators.Where(x => x.TargetType.IsAssignableFrom(type))));
            return _populatorsByType[type];
        }
    }
}