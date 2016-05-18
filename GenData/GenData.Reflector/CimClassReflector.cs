using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenData.Reflector
{
    public class CimClassReflector : ReflectorStrategy
    {
        private readonly string CimNamespace;

        public CimClassReflector():this(null)
        {
            
        }

        public CimClassReflector(Dictionary<Type, TypeMetaInfo> referenceTypeInstancesMappedByType)
            : base(referenceTypeInstancesMappedByType)
        {
            this.CimNamespace = ConfigurationManager.AppSettings.Get("CimNamespace");
        }
        
        public override IEnumerable<TypeMetaInfo> GetTypesFromAssembly(string assemblyLocation)
        {
            if (string.IsNullOrEmpty(assemblyLocation) || !File.Exists(assemblyLocation))
            {
                return null;
            }

            var assembly = Assembly.LoadFrom(assemblyLocation);
            var requestsAndResponses = assembly.GetTypes().Where(type => type.Namespace == this.CimNamespace && ( type.Name.EndsWith("ProcessNewBusinessDepositRequest") || type.Name.EndsWith("ProcessNewBusinessDepositRequest") ))
                .OrderBy(x => x.Name)
                .ToList();

            var requiredTypes = requestsAndResponses.Select(this.GetMetaInfoForType);
            
            return requiredTypes;
        }

    }
}
