using System;
using System.Collections.Generic;

namespace ApiParser
{
    public class DocMember
    {
        public string Summary;
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();
        public Dictionary<string, string> TypeParams = new Dictionary<string, string>();
        public string ReturnsSummary;
    }

    public abstract class DocType
    {
        public string Name;
        public Type Type;
        public string Summary;
        public string Namespace;
    }

    public class EnumTypeDoc : DocType
    {
        public List<EnumDoc> Values = new List<EnumDoc>();
    }

    public class EnumDoc
    {
        public string Name;
        public string Summary;
    }

    public class ClassTypeDoc : DocType
    {
        public bool IsGeneric;
        public bool IsStatic;
        public bool IsAbstract;
        public bool IsSealed;
        public List<MethodDoc> Methods = new List<MethodDoc>();
        public List<PropertyDoc> Properties = new List<PropertyDoc>();
        public List<FieldDoc> Fields = new List<FieldDoc>();
        public List<ConstructorDoc> Constructors = new List<ConstructorDoc>();
        public bool Obsolete;
        public string ObsoleteString;
    }

    public class StructDocType : DocType
    {
        public bool IsGeneric;
        public List<MethodDoc> Methods = new List<MethodDoc>();
        public List<PropertyDoc> Properties = new List<PropertyDoc>();
        public List<FieldDoc> Fields = new List<FieldDoc>();
        public List<ConstructorDoc> Constructors = new List<ConstructorDoc>();
        public bool Obsolete;
        public string ObsoleteString;
    }

    public class ConstructorDoc
    {
        public string Summary;
        public Type Type;
        public List<ParameterDoc> Parameters = new List<ParameterDoc>();
        public bool Obsolete;
        public string ObsoleteString;
    }

    public class MethodDoc
    {
        public string Name;
        public string Summary;
        public Type ReturnType;
        public string ReturnSummary;
        public bool IsStatic;
        public bool IsAbstract;
        public Type InheritedFrom;
        public List<ParameterDoc> Parameters = new List<ParameterDoc>();
        public bool Obsolete;
        public string ObsoleteString;
    }

    public class ParameterDoc
    {
        public Type Type;
        public bool IsOptional;
        public object DefaultValue;
        public bool IsOut;
        public string Name;
        public string SafeName;
        public string Summary;
    }

    public class FieldDoc
    {
        public Type Type;
        public string Name;
        public string Summary;
        public bool IsStatic;
        public Type InheritedFrom;
        public bool Obsolete;
        public string ObsoleteString;
    }

    public class PropertyDoc
    {
        public Type Type;
        public string Name;
        public string Summary;
        public Type InheritedFrom;
        public bool Getter;
        public bool Setter;
        public bool Obsolete;
        public string ObsoleteString;
    }
}
