using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GenData.Reflector
{
    public abstract  class ReflectorStrategy
    {
        public abstract TypeMetaInfo GetMetaInfoForType(Type type);

        protected virtual bool IsCollection(Type type)
        {
            var isCollectionType = type.GetInterfaces().Any(x => x.IsGenericType ? x.GetGenericTypeDefinition() == typeof(IEnumerable<>) : x == typeof(IEnumerable));

            return isCollectionType;
        }
    }
}