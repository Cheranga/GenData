using System.Collections.Generic;
using System.Diagnostics;

namespace GenData.Reflector
{
    [DebuggerDisplay("{Name} :: {TypeName} :: {Value}")]
    public class TypeMetaInfo
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public bool IsClass { get; set; }
        public bool IsGeneric { get; set; }
        public bool IsCollection { get; set; }
        public bool CanDelete { get; set; }
        public string Value { get; set; }

        public List<TypeMetaInfo> Properties { get; set; }

        public TypeMetaInfo Dummy { get; set; }
    }
}
