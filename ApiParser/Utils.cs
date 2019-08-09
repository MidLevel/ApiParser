using System;
using System.Linq;
using System.Reflection;

namespace ApiParser
{
    public static class Utils
    {
        public static string GetPrintableParams(Type type)
        {
            if (type.IsGenericType)
            {
                Type[] parameters = type.GetGenericArguments();
                string args = "";

                if (parameters.Length > 0)
                {
                    args = string.Join(",", parameters.Select(GetPrintableParams)).Replace("+", ".").Replace("&", "@");
                    args = $"{{{args}}}";
                }

                string output = $"{type.GetGenericTypeDefinition().FullName}{args}".Replace("`1", "").Replace("`2", "")
                    .Replace("`3", "").Replace("`4", "").Replace("`5", "").Replace("`6", "")
                    .Replace("`7", "").Replace("`8", "").Replace("`9", "").Replace("`0", "");

                return output;
            }

            return type.IsGenericParameter ? type.Name : (type.FullName != null ? type.FullName.Replace("+", ".") : type.Name);
        }

        public static string PascalToSafeKebab(string pascal)
        {
            string safeKebab = "";
            bool wasLastUpper = false;

            for (int i = 0; i < pascal.Length; i++)
            {
                if (!wasLastUpper && char.IsUpper(pascal[i]))
                {
                    if (i == 0 || i == (pascal.Length - 1))
                    {
                        safeKebab += char.ToLower(pascal[i]);
                        wasLastUpper = true;
                    }
                    else
                    {
                        safeKebab += "-" + char.ToLower(pascal[i]);
                        wasLastUpper = true;
                    }
                }
                else if (wasLastUpper && char.IsUpper(pascal[i]))
                {
                    safeKebab += char.ToLower(pascal[i]);
                    wasLastUpper = true;
                }
                else
                {
                    safeKebab += pascal[i];
                    wasLastUpper = false;
                }
            }

            return safeKebab;
        }

        public static string GetLinkedTypeString(Type type, DocTree tree)
        {
            if (tree.docTypes.Select(x => x.Type).Contains(type))
            {
                return "[``" + GetSafeTypeName(type, false) + "``](" + GetRelativeApiUrl(type) + ")";
            }

            return "``" + GetSafeTypeName(type, false) + "``";
        }

        public static string GetRelativeApiUrl(Type type)
        {
            return ("/" + "api/" + PascalToSafeKebab(GetSafeTypeName(type, false)).Replace("<", "%3C").Replace(">", "%3E") + "/");
        }

        public static string GetRelativeName(Type type)
        {
            return PascalToSafeKebab(GetSafeTypeName(type, false)).Replace("<", "%3C").Replace(">", "%3E").Replace(" ", "");
        }

        public static string GetSafeTypeName(Type type, bool markdown)
        {
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(long)) return "long";
            if (type == typeof(int)) return "int";
            if (type == typeof(short)) return "short";
            if (type == typeof(sbyte)) return "sbyte";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(string)) return "string";
            if (type == typeof(void)) return "void";
            if (type == typeof(object)) return "object";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(ulong[])) return "ulong[]";
            if (type == typeof(uint[])) return "uint[]";
            if (type == typeof(ushort[])) return "ushort[]";
            if (type == typeof(byte[])) return "byte[]";
            if (type == typeof(long[])) return "long[]";
            if (type == typeof(int[])) return "int[]";
            if (type == typeof(short[])) return "short[]";
            if (type == typeof(sbyte[])) return "sbyte[]";
            if (type == typeof(double[])) return "double[]";
            if (type == typeof(float[])) return "float[]";
            if (type == typeof(string[])) return "string[]";
            if (type == typeof(object[])) return "object[]";

            if (type.IsGenericType)
            {
                return (type.Name.Trim('`', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0') + (markdown ? "&lt;" : "<") + string.Join(", ", type.GetTypeInfo().GenericTypeParameters.Select(x => GetSafeTypeName(x, markdown))) + string.Join(", ", type.GenericTypeArguments.Select(x => GetSafeTypeName(x, markdown))) + (markdown ? "&gt;" : ">")).Replace(" ", "");
            }

            return $"{type.Name}";
        }
    }
}
