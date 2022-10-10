using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AYellowpaper.SerializedCollections.Populators
{
    public static class PopulatorCache
    {
        private static List<PopulatorData> _populators;
        private static Dictionary<Type, List<PopulatorData>> _populatorsByType;

        static PopulatorCache()
        {
            _populators = new List<PopulatorData>();
            _populatorsByType = new Dictionary<Type, List<PopulatorData>>();
            var populatorTypes = TypeCache.GetTypesDerivedFrom<Populator>();
            foreach (var populatorType in populatorTypes.Where(x => !x.IsAbstract))
            {
                var attribute = populatorType.GetCustomAttribute<PopulatorAttribute>();
                if (attribute != null)
                    _populators.Add(new PopulatorData(attribute.Name, attribute.TargetType, populatorType));
            }
        }

        public static IReadOnlyList<PopulatorData> GetPopulatorsForType(Type type)
        {
            if (!_populatorsByType.ContainsKey(type))
                _populatorsByType.Add(type, new List<PopulatorData>(_populators.Where(x => x.TargetType.IsAssignableFrom(type))));
            return _populatorsByType[type];
        }
    }
}