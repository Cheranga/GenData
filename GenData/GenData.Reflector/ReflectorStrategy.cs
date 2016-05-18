using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace GenData.Reflector
{
    public abstract  class ReflectorStrategy
    {
        protected readonly Dictionary<Type, TypeMetaInfo> referenceTypeInstancesMappedByType;

        protected ReflectorStrategy(Dictionary<Type, TypeMetaInfo> referenceTypeInstancesMappedByType)
        {
            this.referenceTypeInstancesMappedByType = referenceTypeInstancesMappedByType ?? new Dictionary<Type, TypeMetaInfo>();
        }

        public abstract IEnumerable<TypeMetaInfo> GetTypesFromAssembly(string assemblyLocation);

        protected virtual bool IsCollection(Type type)
        {
            var isCollectionType = type.GetInterfaces().Any(x => x.IsGenericType ? x.GetGenericTypeDefinition() == typeof(IEnumerable<>) : x == typeof(IEnumerable));

            return isCollectionType;
        }

        public virtual object CreateObject(SubmittedTypeData submittedTypeData)
        {
            var cimAssemblyLocation = ConfigurationManager.AppSettings.Get("CIM");
            var cimAssembly = Assembly.LoadFrom(cimAssemblyLocation);

            Func<TypeMetaInfo, object> createInstance = null;

            createInstance = (info) =>
            {
                if ( info == null )
                {
                    return null;
                }

                var typeToCreate = cimAssembly.GetType(info.TypeName, false);

                if ( typeToCreate == null )
                {
                    return null;
                }

                object instance = Activator.CreateInstance(typeToCreate);

                if ( info.Properties != null && info.Properties.Any() )
                {
                    info.Properties.ForEach(prop =>
                    {

                        var propertyInfo = typeToCreate.GetProperty(prop.Name, BindingFlags.Public | BindingFlags.Instance);
                        if ( propertyInfo == null )
                        {
                            // Don't do anything
                        }
                        else
                        {
                            //
                            // Check if it's a string
                            //
                            var propertyType = propertyInfo.PropertyType;
                            var isString = propertyType == typeof(string);
                            var isCollection = IsCollection(propertyType);
                            var isClass = propertyType.IsClass;
                            var isGeneric = propertyType.IsGenericType;

                            var propertyInstance = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;


                            if ( isString )
                            {
                                propertyInstance = prop.Value;
                            }
                            else if ( isCollection )
                            {
                                var arrayElementType = isGeneric ? propertyType.GenericTypeArguments[ 0 ] : propertyType.GetElementType();
                                var array = Array.CreateInstance(arrayElementType, prop.Properties.Count);

                                Enumerable.Range(0, prop.Properties.Count).ToList().ForEach(index =>
                                {
                                    var arrayElementInstance = createInstance(prop.Properties[ index ]);
                                    array.SetValue(arrayElementInstance, index);
                                });

                                if ( propertyType.IsArray /*|| typeof(IEnumerable).IsAssignableFrom(propertyType)*/)
                                {
                                    propertyInstance = array;
                                }
                                else
                                {
                                    var isGenericInterfaceDeclaredCollection = propertyType.IsInterface;

                                    propertyInstance = isGenericInterfaceDeclaredCollection ? array : Activator.CreateInstance(propertyType, array);
                                }
                            }
                            else if ( isClass && isGeneric )
                            {
                                //propertyInstance = Activator.CreateInstance(propertyType);

                                //var genericInstance = createInstance(prop);
                                //propertyInfo.SetValue(propertyInstance,genericInstance);

                                propertyInstance = createInstance(prop);

                            }
                            else if ( isClass )
                            {
                                propertyInstance = createInstance(prop);
                            }
                            else
                            {
                                var typeConverter = TypeDescriptor.GetConverter(propertyType);
                                propertyInstance = typeConverter.IsValid(prop.Value) ? typeConverter.ConvertFromInvariantString(prop.Value) : Activator.CreateInstance(propertyType);
                            }

                            propertyInfo.SetValue(instance, propertyInstance);
                        }
                    });
                }

                return instance;

            };


            var result = createInstance(submittedTypeData.TypeMetaInfo);

            return result;
        }

        public virtual TypeMetaInfo GetMetaInfoForType(Type type)
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

                if ( isStringType )
                {
                    instance = new TypeMetaInfo
                    {
                        Name = propertyInfo.Name,
                        TypeName = propertyType.FullName,//propertyType.AssemblyQualifiedName,
                        IsClass = false,
                        IsCollection = false,
                        IsGeneric = false,
                        Properties = null,
                        Dummy = new TypeMetaInfo
                        {
                            Name = "Child",
                            TypeName = typeof(string).FullName,
                        }
                    };

                    return instance;
                }
                if ( isCollection )
                {
                    var collectionElementType = isGeneric ? propertyType.GetGenericArguments().FirstOrDefault() : propertyType.GetElementType();

                    var collectionElementTypeIsClass = collectionElementType.IsClass && collectionElementType != typeof(string);

                    TypeMetaInfo dummy = null;

                    if ( referenceTypeInstancesMappedByType.ContainsKey(collectionElementType) )
                    {
                        dummy = referenceTypeInstancesMappedByType[ collectionElementType ].GetClone();
                        dummy.CanDelete = true;
                    }
                    else
                    {
                        dummy = new TypeMetaInfo
                        {
                            Name = collectionElementType.Name,
                            TypeName = collectionElementType.FullName,//collectionElementType.AssemblyQualifiedName,
                            IsClass = collectionElementTypeIsClass,
                            IsCollection = IsCollection(collectionElementType),
                            IsGeneric = collectionElementType.IsGenericType,
                            Properties = ( collectionElementTypeIsClass ) ? collectionElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(action).ToList() : null,
                            CanDelete = true
                        };

                        referenceTypeInstancesMappedByType.Add(collectionElementType, dummy);
                    }

                    instance = new TypeMetaInfo
                    {
                        Name = propertyInfo.Name,
                        TypeName = propertyType.FullName,//propertyType.AssemblyQualifiedName,
                        IsClass = propertyType.IsClass,
                        IsCollection = true,
                        IsGeneric = propertyType.IsGenericType,
                        Properties = null,
                        Dummy = dummy
                    };

                    return instance;

                }
                if ( isClass )
                {
                    if ( referenceTypeInstancesMappedByType.ContainsKey(propertyType) )
                    {
                        instance = referenceTypeInstancesMappedByType[ propertyType ].GetClone();
                        instance.Name = propertyInfo.Name;
                        instance.CanDelete = false;
                    }
                    else
                    {
                        var properties = propertyType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                        instance = new TypeMetaInfo
                        {
                            Name = propertyInfo.Name,
                            TypeName = propertyType.FullName,//propertyType.AssemblyQualifiedName,
                            IsClass = propertyType.IsClass,
                            IsCollection = IsCollection(propertyType),
                            IsGeneric = propertyType.IsGenericType,
                            Properties = ( properties != null && properties.Any() ) ? properties.ToList().Select(action).ToList() : null,
                            Dummy = null
                        };

                        referenceTypeInstancesMappedByType.Add(propertyType, instance);

                    }

                    return instance;

                }

                instance = new TypeMetaInfo
                {
                    Name = propertyInfo.Name,
                    TypeName = propertyType.FullName,//propertyType.AssemblyQualifiedName,
                    IsClass = false,
                    IsCollection = false,
                    IsGeneric = propertyType.IsGenericType,
                    Properties = null,
                    Dummy = null
                };

                return instance;
            };


            var typeProperties = ( type.IsClass && type != typeof(string) ) ? type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) : null;
            var objInstance = new TypeMetaInfo
            {
                Name = type.Name,
                TypeName = type.FullName,//type.AssemblyQualifiedName,
                IsClass = type.IsClass,
                IsCollection = IsCollection(type),
                IsGeneric = type.IsGenericType,
                Properties = ( typeProperties != null && typeProperties.Any() ) ? typeProperties.ToList().Select(action).ToList() : null
            };

            return objInstance;
        }
    }
}