using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ApiParser
{
    public class DocTree
    {
        public readonly List<DocType> docTypes = new List<DocType>();

        private DocTree() { }

        public static DocTree Parse(Assembly assembly, XmlDocTree xmlDocTree)
        {
            DocTree docTree = new DocTree();

            foreach (Type type in assembly.ExportedTypes)
            {
                if (type.IsEnum)
                {
                    docTree.ParseEnum(type, xmlDocTree);
                }
                else if (type.IsClass)
                {
                    docTree.ParseClass(type, xmlDocTree);
                }
                else if (type.IsValueType)
                {
                    docTree.ParseStruct(type, xmlDocTree);
                }
            }

            return docTree;
        }


        private void ParseEnum(Type type, XmlDocTree xmlDocTree)
        {
            EnumTypeDoc enumType = new EnumTypeDoc()
            {
                Name = type.Name,
                Type = type,
                Namespace = type.Namespace
            };

            foreach (string name in type.GetEnumNames())
            {
                EnumDoc enumValue = new EnumDoc()
                {
                    Name = name
                };

                DocMember member = xmlDocTree.GetDocumentationEnumValue(type, name);

                if (member != null)
                {
                    enumValue.Summary = member.Summary;
                }

                enumType.Values.Add(enumValue);
            }

            {
                DocMember member = xmlDocTree.GetDocumentation(type);

                if (member != null)
                {
                    enumType.Summary = member.Summary;
                }
            }

            docTypes.Add(enumType);
        }

        private MethodDoc ParseMethod(Type type, MethodInfo method, XmlDocTree xmlDocTree)
        {
            MethodDoc methodDoc = new MethodDoc()
            {
                Name = method.Name,
                ReturnType = method.ReturnType,
                IsStatic = method.IsStatic,
                IsAbstract = method.IsAbstract,
                InheritedFrom = method.DeclaringType == type ? null : method.DeclaringType,
                Obsolete = method.IsDefined(typeof(ObsoleteAttribute), true),
                ObsoleteString = method.GetCustomAttribute<ObsoleteAttribute>(true)?.Message
            };

            DocMember member = xmlDocTree.GetDocumentation(method);

            if (member != null)
            {
                methodDoc.Summary = member.Summary;
                methodDoc.ReturnSummary = member.ReturnsSummary;
            }

            methodDoc.Parameters = ParseParameters(method, member);

            return methodDoc;
        }

        private ConstructorDoc ParseConstructor(Type type, ConstructorInfo constructor, XmlDocTree xmlDocTree)
        {
            ConstructorDoc constructorDoc = new ConstructorDoc()
            {
                Type = type,
                Obsolete = constructor.IsDefined(typeof(ObsoleteAttribute), true),
                ObsoleteString = constructor.GetCustomAttribute<ObsoleteAttribute>(true)?.Message
            };

            DocMember member = xmlDocTree.GetDocumentation(constructor);
            if (member != null) constructorDoc.Summary = member.Summary;

            constructorDoc.Parameters = ParseParameters(constructor, member);

            return constructorDoc;
        }

        private FieldDoc ParseField(Type type, FieldInfo field, XmlDocTree xmlDocTree)
        {
            FieldDoc fieldDoc = new FieldDoc()
            {
                Name = field.Name,
                Type = field.FieldType,
                InheritedFrom = field.DeclaringType == type ? null : field.DeclaringType,
                IsStatic = field.IsStatic,
                Obsolete = field.IsDefined(typeof(ObsoleteAttribute), true),
                ObsoleteString = field.GetCustomAttribute<ObsoleteAttribute>(true)?.Message
            };

            DocMember member = xmlDocTree.GetDocumentation(field);
            if (member != null) fieldDoc.Summary = member.Summary;

            return fieldDoc;
        }

        private PropertyDoc ParseProperty(Type type, PropertyInfo property, XmlDocTree xmlDocTree)
        {
            PropertyDoc propertyDoc = new PropertyDoc()
            {
                Name = property.Name,
                Type = property.PropertyType,
                Getter = property.CanRead,
                Setter = property.CanWrite,
                InheritedFrom = property.DeclaringType == type ? null : property.DeclaringType,
                Obsolete = property.IsDefined(typeof(ObsoleteAttribute), true),
                ObsoleteString = property.GetCustomAttribute<ObsoleteAttribute>(true)?.Message
            };

            DocMember member = xmlDocTree.GetDocumentation(property);
            if (member != null) propertyDoc.Summary = member.Summary;

            return propertyDoc;
        }
        
        private List<ParameterDoc> ParseParameters(MethodBase method, DocMember memberDocs)
        {
            List<ParameterDoc> parameters = new List<ParameterDoc>();
            
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                ParameterDoc parameterDoc = new ParameterDoc()
                {
                    Name = parameter.Name,
                    DefaultValue = parameter.HasDefaultValue ? parameter.DefaultValue : null,
                    IsOptional = parameter.IsOptional,
                    IsOut = parameter.IsOut,
                    Type = parameter.ParameterType,
                    SafeName = Utils.GetSafeTypeName(parameter.ParameterType, false)
                };

                if (memberDocs != null && memberDocs.Parameters.ContainsKey(parameterDoc.Name))
                    parameterDoc.Summary = memberDocs.Parameters[parameterDoc.Name];

                parameters.Add(parameterDoc);
            }

            return parameters;
        }

        private void ParseClass(Type type, XmlDocTree xmlDocTree)
        {
            ClassTypeDoc classType = new ClassTypeDoc()
            {
                Name = type.Name,
                Type = type,
                Namespace = type.Namespace,
                IsGeneric = type.IsGenericType,
                IsStatic = type.IsAbstract && type.IsSealed,
                IsAbstract = type.IsAbstract,
                IsSealed = type.IsSealed,
                Obsolete = type.IsDefined(typeof(ObsoleteAttribute), true),
                ObsoleteString = type.GetCustomAttribute<ObsoleteAttribute>(true)?.Message
            };

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static).Where(x => !x.IsSpecialName))
            {
                classType.Methods.Add(ParseMethod(type, method, xmlDocTree));
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                classType.Fields.Add(ParseField(type, field, xmlDocTree));
            }

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                classType.Properties.Add(ParseProperty(type, property, xmlDocTree));
            }

            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                classType.Constructors.Add(ParseConstructor(type, constructor, xmlDocTree));
            }

            DocMember member = xmlDocTree.GetDocumentation(type);
            if (member != null) classType.Summary = member.Summary;

            docTypes.Add(classType);
        }

        private void ParseStruct(Type type, XmlDocTree xmlDocTree)
        {
            StructDocType structDoc = new StructDocType()
            {
                Name = type.Name,
                Type = type,
                Namespace = type.Namespace,
                IsGeneric = type.IsGenericType,
                Obsolete = type.IsDefined(typeof(ObsoleteAttribute), true),
                ObsoleteString = type.GetCustomAttribute<ObsoleteAttribute>(true)?.Message
            };

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static).Where(x => !x.IsSpecialName))
            {
                structDoc.Methods.Add(ParseMethod(type, method, xmlDocTree));
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                structDoc.Fields.Add(ParseField(type, field, xmlDocTree));
            }

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                structDoc.Properties.Add(ParseProperty(type, property, xmlDocTree));
            }

            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                structDoc.Constructors.Add(ParseConstructor(type, constructor, xmlDocTree));
            }

            DocMember member = xmlDocTree.GetDocumentation(type);
            if (member != null) structDoc.Summary = member.Summary;

            docTypes.Add(structDoc);
        }
    }
}
