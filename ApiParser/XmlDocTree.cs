using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace ApiParser
{
    public class XmlDocTree
    {
        private readonly Dictionary<string, DocMember> documentation = new Dictionary<string, DocMember>();

        private XmlDocTree() { }

        public static XmlDocTree Parse(XmlDocument document)
        {
            XmlDocTree xmlTree = new XmlDocTree();

            XmlNode memberNodes = document.SelectSingleNode(".//members");

            if (memberNodes != null)
            {
                foreach (XmlNode memberNode in memberNodes)
                {
                    if (memberNode.Attributes == null) continue;

                    DocMember member = new DocMember();

                    XmlNode summaryNode = memberNode.SelectSingleNode(".//summary");

                    if (summaryNode != null)
                    {
                        member.Summary = summaryNode.InnerText.Trim();
                    }

                    XmlNode returnsNode = memberNode.SelectSingleNode(".//returns");

                    if (returnsNode != null)
                    {
                        member.ReturnsSummary = returnsNode.InnerText.Trim();
                    }

                    XmlNodeList paramNodes = memberNode.SelectNodes(".//param");

                    if (paramNodes != null)
                    {
                        foreach (XmlNode parameterNode in paramNodes)
                        {
                            if (parameterNode.Attributes == null) continue;

                            member.Parameters.Add(parameterNode.Attributes["name"].Value, parameterNode.InnerText.Trim());
                        }
                    }

                    XmlNode typeParamNodes = memberNode.SelectSingleNode(".//typeparam");

                    if (typeParamNodes != null)
                    {
                        foreach (XmlNode typeParamNode in typeParamNodes)
                        {
                            if (typeParamNode.Attributes == null) continue;

                            member.TypeParams.Add(typeParamNode.Attributes["name"].Value, typeParamNode.InnerText.Trim());
                        }
                    }

                    xmlTree.documentation.Add(memberNode.Attributes["name"].Value, member);
                }
            }

            return xmlTree;
        }

        public DocMember GetDocumentation(string name)
        {
            if (!documentation.ContainsKey(name)) return null;

            return documentation[name];
        }

        public DocMember GetDocumentation(MethodInfo o)
        {
            string args = "";
            ParameterInfo[] parameters = o.GetParameters();

            if (parameters.Length > 0)
            {
                args = string.Join(",", parameters.Select(x => Utils.GetPrintableParams(x.ParameterType))).Replace("+", ".").Replace("&", "@");
                args = $"({args})";
            }

            return GetDocumentation($"M:{o.DeclaringType.FullName.Replace("+", ".").Replace("&", "@")}.{o.Name}{args}");
        }

        public DocMember GetDocumentation(ConstructorInfo o)
        {
            string args = "";
            ParameterInfo[] parameters = o.GetParameters();

            if (parameters.Length > 0)
            {
                args = string.Join(",", parameters.Select(x => x.ParameterType.FullName)).Replace("+", ".").Replace("&", "@");
                args = $"({args})";
            }

            return GetDocumentation($"M:{o.DeclaringType.FullName.Replace("+", ".").Replace("&", "@")}.#ctor{args}");
        }

        public DocMember GetDocumentation(PropertyInfo o)
        {
            return GetDocumentation($"P:{o.DeclaringType.FullName.Replace("+", ".").Replace("&", "@")}.{o.Name}");
        }

        public DocMember GetDocumentation(FieldInfo o)
        {
            return GetDocumentation($"F:{o.DeclaringType.FullName.Replace("+", ".").Replace("&", "@")}.{o.Name}");
        }

        public DocMember GetDocumentation(Type o)
        {
            return GetDocumentation($"T:{o.FullName.Replace("+", ".").Replace("&", "@")}");
        }

        public DocMember GetDocumentationEnumValue(Type enumType, string enumName)
        {
            return GetDocumentation($"F:{enumType.FullName.Replace("+", ".").Replace("&", "@")}.{enumName}");
        }
    }
}
