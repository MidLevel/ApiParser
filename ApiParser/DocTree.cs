using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                IsSealed = type.IsSealed
            };

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static).Where(x => !x.IsSpecialName))
            {
                MethodDoc methodDoc = new MethodDoc()
                {
                    Name = method.Name,
                    ReturnType = method.ReturnType,
                    IsStatic = method.IsStatic,
                    IsAbstract = method.IsAbstract,
                    InheritedFrom = method.DeclaringType == type ? null : method.DeclaringType
                };

                DocMember member = xmlDocTree.GetDocumentation(method);

                if (member != null)
                {
                    methodDoc.Summary = member.Summary;
                    methodDoc.ReturnSummary = member.ReturnsSummary;
                }


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

                    if (member != null && member.Parameters.ContainsKey(parameterDoc.Name))
                        parameterDoc.Summary = member.Parameters[parameterDoc.Name];

                    methodDoc.Parameters.Add(parameterDoc);
                }


                classType.Methods.Add(methodDoc);
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                FieldDoc fieldDoc = new FieldDoc()
                {
                    Name = field.Name,
                    Type = field.FieldType,
                    InheritedFrom = field.DeclaringType == type ? null : field.DeclaringType,
                    IsStatic = field.IsStatic
                };

                DocMember member = xmlDocTree.GetDocumentation(field);
                if (member != null) fieldDoc.Summary = member.Summary;

                classType.Fields.Add(fieldDoc);
            }

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                PropertyDoc propertyDoc = new PropertyDoc()
                {
                    Name = property.Name,
                    Type = property.PropertyType,
                    Getter = property.CanRead,
                    Setter = property.CanWrite,
                    InheritedFrom = property.DeclaringType == type ? null : property.DeclaringType,
                    IsStatic = false // TODO
                };

                DocMember member = xmlDocTree.GetDocumentation(property);
                if (member != null) propertyDoc.Summary = member.Summary;

                classType.Properties.Add(propertyDoc);
            }

            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                ConstructorDoc constructorDoc = new ConstructorDoc()
                {
                    Type = type
                };

                DocMember member = xmlDocTree.GetDocumentation(constructor);
                if (member != null) constructorDoc.Summary = member.Summary;

                foreach (ParameterInfo parameter in constructor.GetParameters())
                {
                    ParameterDoc parameterDoc = new ParameterDoc()
                    {
                        Name = parameter.Name,
                        Type = parameter.ParameterType
                    };

                    if (member != null && member.Parameters.ContainsKey(parameterDoc.Name))
                        parameterDoc.Summary = member.Parameters[parameterDoc.Name];

                    constructorDoc.Parameters.Add(parameterDoc);
                }

                classType.Constructors.Add(constructorDoc);
            }


            {
                DocMember member = xmlDocTree.GetDocumentation(type);
                if (member != null) classType.Summary = member.Summary;
            }

            docTypes.Add(classType);
        }

        private void ParseStruct(Type type, XmlDocTree xmlDocTree)
        {
            StructDocType structDoc = new StructDocType()
            {
                Name = type.Name,
                Type = type,
                Namespace = type.Namespace,
                IsGeneric = type.IsGenericType
            };

            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static).Where(x => !x.IsSpecialName))
            {
                MethodDoc methodDoc = new MethodDoc()
                {
                    Name = method.Name,
                    ReturnType = method.ReturnType,
                    IsStatic = method.IsStatic,
                    IsAbstract = method.IsAbstract,
                    InheritedFrom = method.DeclaringType == type ? null : method.DeclaringType
                };

                DocMember member = xmlDocTree.GetDocumentation(method);

                if (member != null)
                {
                    methodDoc.Summary = member.Summary;
                    methodDoc.ReturnSummary = member.ReturnsSummary;
                }


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

                    if (member != null && member.Parameters.ContainsKey(parameterDoc.Name))
                        parameterDoc.Summary = member.Parameters[parameterDoc.Name];

                    methodDoc.Parameters.Add(parameterDoc);
                }


                structDoc.Methods.Add(methodDoc);
            }

            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                FieldDoc fieldDoc = new FieldDoc()
                {
                    Name = field.Name,
                    Type = field.FieldType,
                    InheritedFrom = field.DeclaringType == type ? null : field.DeclaringType,
                    IsStatic = field.IsStatic
                };

                DocMember member = xmlDocTree.GetDocumentation(field);
                if (member != null) fieldDoc.Summary = member.Summary;

                structDoc.Fields.Add(fieldDoc);
            }

            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                PropertyDoc propertyDoc = new PropertyDoc()
                {
                    Name = property.Name,
                    Type = property.PropertyType,
                    Getter = property.CanRead,
                    Setter = property.CanWrite,
                    InheritedFrom = property.DeclaringType == type ? null : property.DeclaringType,
                    IsStatic = false // TODO
                };

                DocMember member = xmlDocTree.GetDocumentation(property);
                if (member != null) propertyDoc.Summary = member.Summary;

                structDoc.Properties.Add(propertyDoc);
            }

            foreach (ConstructorInfo constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static))
            {
                ConstructorDoc constructorDoc = new ConstructorDoc()
                {
                    Type = type
                };

                DocMember member = xmlDocTree.GetDocumentation(constructor);
                if (member != null) constructorDoc.Summary = member.Summary;

                foreach (ParameterInfo parameter in constructor.GetParameters())
                {
                    ParameterDoc parameterDoc = new ParameterDoc()
                    {
                        Name = parameter.Name,
                        Type = parameter.ParameterType
                    };

                    if (member != null && member.Parameters.ContainsKey(parameterDoc.Name))
                        parameterDoc.Summary = member.Parameters[parameterDoc.Name];

                    constructorDoc.Parameters.Add(parameterDoc);
                }

                structDoc.Constructors.Add(constructorDoc);
            }


            {
                DocMember member = xmlDocTree.GetDocumentation(type);
                if (member != null) structDoc.Summary = member.Summary;
            }

            docTypes.Add(structDoc);
        }
    }
}
