using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ApiParser
{
    public class Printer
    {
        public static void Print(DocTree tree, string yamlPath, string apiFolder)
        {
            List<DocType> apis = new List<DocType>();

            for (int i = 0; i < tree.docTypes.Count; i++)
            {
                if (tree.docTypes[i] is ClassTypeDoc)
                {
                    ClassTypeDoc type = (ClassTypeDoc)tree.docTypes[i];

                    apis.Add(type);

                    StringBuilder b = new StringBuilder();
                    b.AppendLine("---");
                    b.AppendLine("title: " + Utils.GetSafeTypeName(type.Type, true));
                    b.AppendLine("permalink: " + Utils.GetRelativeApiUrl(type.Type, false));
                    b.AppendLine("---");
                    b.AppendLine();

                    PrintClass(b, type, tree);

                    File.WriteAllText(Path.Combine(apiFolder, Utils.GetSafeTypeName(type.Type, false).Replace("<", "_").Replace(">", "_") + ".md"), b.ToString());
                }
                else if (tree.docTypes[i] is EnumTypeDoc)
                {
                    EnumTypeDoc type = (EnumTypeDoc)tree.docTypes[i];

                    apis.Add(type);

                    StringBuilder b = new StringBuilder();
                    b.AppendLine("---");
                    b.AppendLine("title: " + Utils.GetSafeTypeName(type.Type, true));
                    b.AppendLine("permalink: " + Utils.GetRelativeApiUrl(type.Type, false));
                    b.AppendLine("---");
                    b.AppendLine();

                    PrintEnum(b, type);

                    File.WriteAllText(Path.Combine(apiFolder, Utils.GetSafeTypeName(type.Type, false).Replace("<", "_").Replace(">", "_") + ".md"), b.ToString());
                }
                else if (tree.docTypes[i] is StructDocType)
                {
                    StructDocType type = (StructDocType)tree.docTypes[i];

                    apis.Add(type);

                    StringBuilder b = new StringBuilder();
                    b.AppendLine("---");
                    b.AppendLine("title: " + Utils.GetSafeTypeName(type.Type, true));
                    b.AppendLine("permalink: " + Utils.GetRelativeApiUrl(type.Type, false));
                    b.AppendLine("---");
                    b.AppendLine();

                    PrintStruct(b, type, tree);

                    File.WriteAllText(Path.Combine(apiFolder, Utils.GetSafeTypeName(type.Type, false).Replace("<", "_").Replace(">", "_") + ".md"), b.ToString());
                }
            }

            StringBuilder builder = new StringBuilder();
            WriteYaml(builder, apis);

            File.WriteAllText(yamlPath, builder.ToString());
        }

        private static void WriteYaml(StringBuilder b, List<DocType> types)
        {
            b.AppendLine("- title: MLAPI.dll");
            b.AppendLine("  api:");
            b.AppendLine("  - home");

            for (int i = 0; i < types.Count; i++)
            {
                b.AppendLine("  - " + Utils.GetRelativeName(types[i].Type));
            }

        }

        private static void PrintEnum(StringBuilder b, EnumTypeDoc type)
        {
            b.AppendLine("<div style=\"line-height: 1;\">");
            b.AppendLine("\t<h2 markdown=\"1\">" + Utils.GetSafeTypeName(type.Type, true) + " ``enum``</h2>");
            b.AppendLine("\t<p style=\"font-size: 20px;\"><b>Namespace:</b> " + type.Namespace + "</p>");
            b.AppendLine("\t<p style=\"font-size: 20px;\"><b>Assembly:</b> MLAPI.dll</p>");
            b.AppendLine("</div>");
            b.AppendLine("<p>" + type.Summary + "</p>");

            b.AppendLine("<div>");
            b.AppendLine("\t<h3 markdown=\"1\">Enum Values</h3>");

            for (int i = 0; i < type.Values.Count; i++)
            {
                b.AppendLine("\t<div>");
                b.AppendLine("\t\t<h4 markdown=\"1\"><b>``" + type.Values[i].Name + "``</b></h4>");
                b.AppendLine("\t\t<p>" + type.Values[i].Summary + "</p>");
                b.AppendLine("\t</div>");
            }

            b.AppendLine("</div>");
        }

        private static void PrintClass(StringBuilder b, ClassTypeDoc type, DocTree tree)
        {
            b.AppendLine("<div style=\"line-height: 1;\">");
            b.AppendLine("\t<h2 markdown=\"1\">" + Utils.GetSafeTypeName(type.Type, true) + " ``class``" + (type.Obsolete ? " <small><span class=\"label label-warning\" title=\"" + type.ObsoleteString + "\">Obsolete</span></small>" : "") + "</h2>");
            b.AppendLine("\t<p style=\"font-size: 20px;\"><b>Namespace:</b> " + type.Namespace + "</p>");
            b.AppendLine("\t<p style=\"font-size: 20px;\"><b>Assembly:</b> MLAPI.dll</p>");
            b.AppendLine("</div>");

            if (type.Summary != null)
            {
                b.AppendLine("<p>" + type.Summary + "</p>");
                b.AppendLine();
            }
            
            PrintProperties(b, type.Properties, tree);
            PrintFields(b, type.Fields, tree);
            PrintConstructors(b, type.Constructors, tree);
            PrintMethods(b, type.Methods, tree);
            
        }

        private static void PrintStruct(StringBuilder b, StructDocType type, DocTree tree)
        {
            b.AppendLine("<div style=\"line-height: 1;\">");
            b.AppendLine("\t<h2 markdown=\"1\">" + Utils.GetSafeTypeName(type.Type, true) + " ``struct``" + (type.Obsolete ? " <small><span class=\"label label-warning\" title=\"" + type.ObsoleteString + "\">Obsolete</span></small>" : "") + "</h2>");
            b.AppendLine("\t<p style=\"font-size: 20px;\"><b>Namespace:</b> " + type.Namespace + "</p>");
            b.AppendLine("\t<p style=\"font-size: 20px;\"><b>Assembly:</b> MLAPI.dll</p>");
            b.AppendLine("</div>");

            if (type.Summary != null)
            {
                b.AppendLine("<p>" + type.Summary + "</p>");
                b.AppendLine();
            }

            PrintProperties(b, type.Properties, tree);
            PrintFields(b, type.Fields, tree);
            PrintConstructors(b, type.Constructors, tree);
            PrintMethods(b, type.Methods, tree);
        }

        private static void PrintMethods(StringBuilder b, List<MethodDoc> methods, DocTree tree)
        {
            StringBuilder publicMethods = new StringBuilder();
            bool hasPublicMethods = false;
            StringBuilder staticMethods = new StringBuilder();
            bool hasStaticMethods = false;
            StringBuilder inheritedMethods = new StringBuilder();
            bool hasInheritedMethods = false;

            foreach (MethodDoc method in methods)
            {
                string methodString = "public " + (method.IsStatic ? "static " : "") + Utils.GetLinkedTypeString(method.ReturnType, tree, true) + " " + method.Name + "(";

                for (int j = 0; j < method.Parameters.Count; j++)
                {
                    methodString += Utils.GetLinkedTypeString(method.Parameters[j].Type, tree, true) + " " + method.Parameters[j].Name;
                    if (j != method.Parameters.Count - 1) methodString += ", ";
                }

                methodString += ");";

                StringBuilder methodBuilder = null;

                if (method.IsStatic)
                {
                    hasStaticMethods = true;
                    methodBuilder = staticMethods;
                }
                else if (method.InheritedFrom != null)
                {
                    hasInheritedMethods = true;
                    methodBuilder = inheritedMethods;
                }
                else
                {
                    hasPublicMethods = true;
                    methodBuilder = publicMethods;
                }

                methodBuilder.AppendLine("\t<div style=\"line-height: 1;\">");
                methodBuilder.AppendLine("\t\t<h4 markdown=\"1\"><b>" + methodString + "</b>" + (method.Obsolete ? " <small><span class=\"label label-warning\" title=\"" + method.ObsoleteString + "\">Obsolete</span></small>" : "") + "</h4>");

                if (method.InheritedFrom != null)
                {
                    methodBuilder.AppendLine("\t\t<h5 markdown=\"1\">Inherited from: " + Utils.GetLinkedTypeString(method.InheritedFrom, tree, true) + "</h5>");
                }


                if (method.Summary != null)
                {
                    methodBuilder.AppendLine("\t\t<p>" + method.Summary + "</p>");
                }

                if (method.Parameters.Count > 0)
                {
                    methodBuilder.AppendLine("\t\t<h5><b>Parameters</b></h5>");

                    for (int j = 0; j < method.Parameters.Count; j++)
                    {
                        methodBuilder.AppendLine("\t\t<div>");
                        methodBuilder.AppendLine("\t\t\t<p style=\"font-size: 20px; color: #444;\" markdown=\"1\">" + Utils.GetLinkedTypeString(method.Parameters[j].Type, tree, true) + " " + method.Parameters[j].Name + "</p>");

                        if (method.Parameters[j].Summary != null)
                        {
                            methodBuilder.AppendLine("\t\t\t<p>" + method.Parameters[j].Summary + "</p>");
                        }

                        methodBuilder.AppendLine("\t\t</div>");
                    }
                }

                if (method.ReturnType != typeof(void) && method.ReturnSummary != null)
                {
                    methodBuilder.AppendLine("\t\t<h5 markdown=\"1\"><b>Returns " + Utils.GetLinkedTypeString(method.ReturnType, tree, true) + "</b></h5>");

                    methodBuilder.AppendLine("\t\t<div>");
                    methodBuilder.AppendLine("\t\t\t<p>" + method.ReturnSummary + "</p>");
                    methodBuilder.AppendLine("\t\t</div>");
                }

                methodBuilder.AppendLine("\t</div>");

                if (method != methods.Last())
                {
                    methodBuilder.AppendLine("\t<br>");
                }
            }

            if (hasPublicMethods)
            {
                b.AppendLine("<div>");
                b.AppendLine("\t<h3 markdown=\"1\">Public Methods</h3>");
                b.Append(publicMethods);
                b.AppendLine("</div>");
                b.AppendLine("<br>");
            }

            if (hasStaticMethods)
            {
                b.AppendLine("<div>");
                b.AppendLine("\t<h3 markdown=\"1\">Public Static Methods</h3>");
                b.Append(staticMethods);
                b.AppendLine("</div>");
                b.AppendLine("<br>");
            }

            if (hasInheritedMethods)
            {
                b.AppendLine("<div>");
                b.AppendLine("\t<h3 markdown=\"1\">Inherited Methods</h3>");
                b.Append(inheritedMethods);
                b.AppendLine("</div>");
                b.AppendLine("<br>");
            }
        }

        private static void PrintConstructors(StringBuilder b, List<ConstructorDoc> constructors, DocTree tree)
        {
            if (constructors.Count == 0) return;

            b.AppendLine("<div>");
            b.AppendLine("\t<h3>Public Constructors</h3>");

            foreach (ConstructorDoc constructor in constructors)
            {
                b.AppendLine("\t<div style=\"line-height: 1; \">");
                string methodString = "public " + Utils.GetLinkedTypeString(constructor.Type, tree, true) + "(";

                for (int j = 0; j < constructor.Parameters.Count; j++)
                {
                    methodString += Utils.GetLinkedTypeString(constructor.Parameters[j].Type, tree, true) + " " + constructor.Parameters[j].Name;
                    if (j != constructor.Parameters.Count - 1) methodString += ", ";
                }

                methodString += ");";

                b.AppendLine("\t\t<h4 markdown=\"1\"><b>" + methodString + "</b>" + (constructor.Obsolete ? " <small><span class=\"label label-warning\" title=\"" + constructor.ObsoleteString + "\">Obsolete</span></small>" : "") + "</h4>");

                if (constructor.Summary != null)
                {
                    b.AppendLine("\t\t<p>" + constructor.Summary + "</p>");
                }

                b.AppendLine("\t</div>");

                if (constructor.Parameters.Count > 0)
                {
                    b.AppendLine("\t\t<h5><b>Parameters</b></h5>");

                    for (int j = 0; j < constructor.Parameters.Count; j++)
                    {
                        b.AppendLine("\t\t<div>");
                        b.AppendLine("\t\t\t<p style=\"font-size: 20px; color: #444;\" markdown=\"1\">" + Utils.GetLinkedTypeString(constructor.Parameters[j].Type, tree, true) + " " + constructor.Parameters[j].Name + "</p>");

                        if (constructor.Parameters[j].Summary != null)
                        {
                            b.AppendLine("\t\t\t<p>" + constructor.Parameters[j].Summary + "</p>");
                        }

                        b.AppendLine("\t\t</div>");
                    }
                }
            }

            b.AppendLine("</div>");
            b.AppendLine("<br>");
        }

        private static void PrintFields(StringBuilder b, List<FieldDoc> fields, DocTree tree)
        {
            if (fields.Count == 0) return;

            StringBuilder publicFields = new StringBuilder();
            bool hasPublicFields = false;
            StringBuilder inheritedFields = new StringBuilder();
            bool hasInheritedFields = false;

            foreach (FieldDoc field in fields)
            {
                StringBuilder fieldBuilder = null;

                if (field.InheritedFrom != null)
                {
                    hasInheritedFields = true;
                    fieldBuilder = inheritedFields;
                }
                else
                {
                    hasPublicFields = true;
                    fieldBuilder = publicFields;
                }

                fieldBuilder.AppendLine("\t<div style=\"line-height: 1;\">");
                fieldBuilder.AppendLine("\t\t<h4 markdown=\"1\"><b>public " + Utils.GetLinkedTypeString(field.Type, tree, true) + " " + field.Name + ";</b>" + (field.Obsolete ? " <small><span class=\"label label-warning\" title=\"" + field.ObsoleteString + "\">Obsolete</span></small>" : "") + "</h4>");

                if (field.InheritedFrom != null)
                {
                    fieldBuilder.AppendLine("\t\t<h5 markdown=\"1\">Inherited from: " + Utils.GetLinkedTypeString(field.InheritedFrom, tree, true) + "</h5>");
                }

                if (field.Summary != null)
                {
                    fieldBuilder.AppendLine("\t\t<p>" + field.Summary + "</p>");
                }

                fieldBuilder.AppendLine("\t</div>");
            }

            if (hasPublicFields)
            {
                b.AppendLine("<div>");
                b.AppendLine("\t<h3 markdown=\"1\">Public Fields</h3>");
                b.Append(publicFields);
                b.AppendLine("</div>");
                b.AppendLine("<br>");
            }

            if (hasInheritedFields)
            {
                b.AppendLine("<div>");
                b.AppendLine("\t<h3 markdown=\"1\">Inherited Fields</h3>");
                b.Append(inheritedFields);
                b.AppendLine("</div>");
                b.AppendLine("<br>");
            }
        }


        private static void PrintProperties(StringBuilder b, List<PropertyDoc> properties, DocTree tree)
        {
            if (properties.Count == 0) return;

            StringBuilder publicProperties = new StringBuilder();
            bool hasPublicProperties = false;
            StringBuilder inheritedProperties = new StringBuilder();
            bool hasInheritedProperties = false;

            foreach (PropertyDoc property in properties)
            {
                StringBuilder propertyBuilder = null;

                if (property.InheritedFrom != null)
                {
                    hasInheritedProperties = true;
                    propertyBuilder = inheritedProperties;
                }
                else
                {
                    hasPublicProperties = true;
                    propertyBuilder = publicProperties;
                }

                propertyBuilder.AppendLine("\t<div style=\"line-height: 1;\">");
                propertyBuilder.AppendLine("\t\t<h4 markdown=\"1\"><b>public " + Utils.GetLinkedTypeString(property.Type, tree, true) + " " + property.Name + " { " + (property.Getter ? "get; " : "") + (property.Setter ? "set; " : "") + "}</b>" + (property.Obsolete ? " <small><span class=\"label label-warning\" title=\"" + property.ObsoleteString + "\">Obsolete</span></small>" : "") + "</h4>");

                if (property.InheritedFrom != null)
                {
                    propertyBuilder.AppendLine("\t\t<h5 markdown=\"1\">Inherited from: " + Utils.GetLinkedTypeString(property.InheritedFrom, tree, true) + "</h5>");
                }

                if (property.Summary != null)
                {
                    propertyBuilder.AppendLine("\t\t<p>" + property.Summary + "</p>");
                }

                propertyBuilder.AppendLine("\t</div>");
            }

            if (hasPublicProperties)
            {
                b.AppendLine("<div>");
                b.AppendLine("\t<h3 markdown=\"1\">Public Properties</h3>");
                b.Append(publicProperties);
                b.AppendLine("</div>");
                b.AppendLine("<br>");
            }

            if (hasInheritedProperties)
            {
                b.AppendLine("<div>");
                b.AppendLine("\t<h3 markdown=\"1\">Inherited Properties</h3>");
                b.Append(inheritedProperties);
                b.AppendLine("</div>");
                b.AppendLine("<br>");
            }
        }
    }
}
