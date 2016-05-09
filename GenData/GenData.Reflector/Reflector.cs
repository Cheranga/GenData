using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GenData.Reflector
{
    public class Reflector
    {
        private readonly Dictionary<Type, TypeMetaInfo> referenceTypeInstancesMappedByType;

        public Reflector()
        {
            this.referenceTypeInstancesMappedByType = new Dictionary<Type, TypeMetaInfo>();
        } 

        public static bool IsCollection(Type type)
        {
            var isCollectionType = type.GetInterfaces().Any(x => x.IsGenericType ? x.GetGenericTypeDefinition() == typeof(IEnumerable<>) : x == typeof(IEnumerable));

            return isCollectionType;
        }

        public TypeMetaInfo GetMetaInfoForType(Type type)
        {
            Func<PropertyInfo, TypeMetaInfo> action = null;

            action = propertyInfo =>
            {
                var propertyType = propertyInfo.PropertyType;
                var isClass = propertyType.IsClass;
                var isStringType = propertyType == typeof(string);
                var isCollection = IsCollection(propertyType);
                var isGeneric = propertyType.IsGenericType;
                TypeMetaInfo instance = null;
                TypeMetaInfo dummy = null;

                if (isStringType)
                {
                    instance = new TypeMetaInfo
                    {
                        Name = propertyInfo.Name,
                        TypeName = propertyType.FullName,
                        IsClass = false,
                        IsCollection = false,
                        IsGeneric = false,
                        Properties = null,
                        Dummy = new TypeMetaInfo
                        {
                            Name = "Child",
                            TypeName = typeof (string).FullName,
                        }
                    };

                    return instance;
                }
                if (isCollection)
                {
                    var collectionElementType = isGeneric ? propertyType.GetGenericArguments().FirstOrDefault() : propertyType.GetElementType();

                    var collectionElementTypeIsClass = collectionElementType.IsClass && collectionElementType != typeof (string);

                    if (referenceTypeInstancesMappedByType.ContainsKey(collectionElementType))
                    {
                        dummy = referenceTypeInstancesMappedByType[collectionElementType];
                    }
                    else
                    {
                        dummy = new TypeMetaInfo
                        {
                            Name = collectionElementType.Name,
                            TypeName = collectionElementType.FullName,
                            IsClass = collectionElementTypeIsClass,
                            IsCollection = IsCollection(collectionElementType),
                            IsGeneric = collectionElementType.IsGenericType,
                            Properties = (collectionElementTypeIsClass) ? collectionElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(action).ToList() : null,
                            CanDelete = true
                        };

                        referenceTypeInstancesMappedByType.Add(collectionElementType, dummy);
                    }

                    instance = new TypeMetaInfo
                    {
                        Name = propertyInfo.Name,
                        TypeName = propertyType.FullName,
                        IsClass = propertyType.IsClass,
                        IsCollection = true,
                        IsGeneric = propertyType.IsGenericType,
                        Properties = null,
                        Dummy = dummy
                    };

                    return instance;

                }
                if (isClass)
                {
                    if (referenceTypeInstancesMappedByType.ContainsKey(propertyType))
                    {
                        instance = referenceTypeInstancesMappedByType[propertyType];
                    }
                    else
                    {
                        var properties = propertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                        instance = new TypeMetaInfo
                        {
                            Name = propertyInfo.Name,
                            TypeName = propertyType.FullName,
                            IsClass = propertyType.IsClass,
                            IsCollection = IsCollection(propertyType),
                            IsGeneric = propertyType.IsGenericType,
                            Properties = (properties != null && properties.Any()) ? properties.ToList().Select(action).ToList() : null,
                            Dummy = null
                        };

                        referenceTypeInstancesMappedByType.Add(propertyType, instance);

                    }

                    return instance;

                }

                instance = new TypeMetaInfo
                {
                    Name = propertyInfo.Name,
                    TypeName = propertyType.FullName,
                    IsClass = false,
                    IsCollection = false,
                    IsGeneric = propertyType.IsGenericType,
                    Properties = null,
                    Dummy = null
                };

                return instance;
            };


            var typeProperties = (type.IsClass && type != typeof(string)) ? type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) : null;
            var objInstance = new TypeMetaInfo
            {
                Name = type.Name,
                TypeName = type.FullName,
                IsClass = type.IsClass,
                IsCollection = IsCollection(type),
                IsGeneric = type.IsGenericType,
                Properties = (typeProperties != null && typeProperties.Any()) ? typeProperties.ToList().Select(action).ToList() : null
            };

            return objInstance;

        }

        public T ToObject<T>(TypeMetaInfo metaInfo) where T:class 
        {
            if (metaInfo == null)
            {
                return default(T);
            }




            return default(T);
        }
    }
}
