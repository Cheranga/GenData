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
                            TypeName = collectionElementType.AssemblyQualifiedName,
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
                        TypeName = propertyType.AssemblyQualifiedName,
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
                            TypeName = propertyType.AssemblyQualifiedName,
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
                    TypeName = propertyType.AssemblyQualifiedName,
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
                TypeName = type.AssemblyQualifiedName,
                IsClass = type.IsClass,
                IsCollection = IsCollection(type),
                IsGeneric = type.IsGenericType,
                Properties = (typeProperties != null && typeProperties.Any()) ? typeProperties.ToList().Select(action).ToList() : null
            };

            return objInstance;

        }

        public object CreateObject(string assemblyPath, TypeMetaInfo typeMetaInfo)
        {
            if (typeMetaInfo == null)
            {
                return null;
            }

            var assembly = Assembly.LoadFrom(assemblyPath);
            if (assembly == null)
            {
                return new NullReferenceException("Assembly cannot be loaded");
            }

            if (typeMetaInfo.IsClass)
            {
                var instance = Activator.CreateInstance(Type.GetType(typeMetaInfo.TypeName));

            }

            //Func<Assembly, TypeMetaInfo, object> getObjectFunc = (ass, metaInfo) =>
            //{
            //    if (ass == null || metaInfo == null)
            //    {
            //        throw new NullReferenceException("metaInfo is null");
            //    }

            //    var isClass = metaInfo.IsClass;
            //    if (isClass)
            //    {
            //        var type = ass.GetType(metaInfo.TypeName);
            //        type.defa

            //    }
                
            //    return null;
            //};


            return null;
        }
    }
}
