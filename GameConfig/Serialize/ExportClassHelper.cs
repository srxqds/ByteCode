/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：May 31 2016
// 模块描述：
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Text;
using ByteBuilder;

namespace Serialize
{
    //导出代码辅助函数
    public static class ExportClassHelper
    {
        public static List<ClassDefine> globalClassDefines = new List<ClassDefine>();
        public static int indent;
        
        public static string GetDebugString(FieldDefine field, Language lanaguage)
        {
            return string.Empty;
        }

        public static List<string> GetNamespaces(Language language, string type,List<ClassDefine> queryClasss)
        {
            List<string> namespaceList = new List<string>();
            List<object> nestArray = CodeExporterHelper.TypeName2NestArray(type);
            object element = nestArray[0];
            if (element is string)
            {
                string prefixType = element as string;
                string typeForGetArrayDemision = type;
                int array = CodeExporterHelper.GetArraySuffix(ref prefixType);
                if (array == 0)
                    array = CodeExporterHelper.GetArraySuffix(ref typeForGetArrayDemision);
                if (array > 0) //Array
                {
                    namespaceList.AddRange(GetNamespaces(language, CodeExporterHelper.GetArrayElementTypeName(type),queryClasss));
                }
                else if (prefixType == CodeGenConstants.ComplexType[1])//List
                {
                    if (language == Language.CSharp)
                    {
                        namespaceList.Add("System.Collections.Generic");
                    }
                    else if (language == Language.Java)
                    {
                        namespaceList.Add("java.util.List");
                        namespaceList.Add("java.util.ArrayList");
                    }
                    namespaceList.AddRange(GetNamespaces(language, CodeExporterHelper.GetListGenericTypeName(type), queryClasss));
                }
                else if (prefixType == CodeGenConstants.ComplexType[2])//Dictionary
                {
                    if (language == Language.CSharp)
                    {
                        namespaceList.Add("System.Collections.Generic");
                    }
                    else if (language == Language.Java)
                    {
                        namespaceList.Add("java.util.HashMap");
                    }
                    string[] genericTypes = CodeExporterHelper.GetDictionaryGenericTypeName(type);
                    namespaceList.AddRange(GetNamespaces(language, genericTypes[0], queryClasss));
                    namespaceList.AddRange(GetNamespaces(language, genericTypes[1], queryClasss));

                }
                else if (Array.IndexOf(CodeGenConstants.BaseType, prefixType) == -1) //custom class
                {
                    namespaceList.Add(BufferCodeExporter.GetNamespaceStatement(language, "", NamespaceMode.None));
                    namespaceList.Add("ByteBuilder");
                }
            }
            return namespaceList;
        }

        //增加命名空间:xxx.xxx.xxx 这种形式，然后xxx都是小写
        public static bool ExportNamespace(StringBuilder sb, ClassDefine classNode, Language language)
        {
            string namesapceStatement = BufferCodeExporter.GetNamespaceStatement(language,classNode.decoration,NamespaceMode.Declare);
            HashSet<string> usingNamesapceSet = new HashSet<string>();
            foreach (var fieldNode in classNode.fieldList)
            {
                var usingNamespaces = GetNamespaces(language, fieldNode.decoration, globalClassDefines);
                foreach (var usingNamespace in usingNamespaces)
                {
                    if (!usingNamesapceSet.Contains(usingNamespace))
                    {
                        usingNamesapceSet.Add(usingNamespace);
                    }
                }
            }
            if (language == Language.Java)
            {
                sb.AppendIndentLine(namesapceStatement);
                sb.AppendLine("");

                foreach (var usingNamespace in usingNamesapceSet)
                {
                    if (!string.IsNullOrEmpty(usingNamespace))
                    {
                        sb.AppendIndentLine(BufferCodeExporter.GetNamespaceStatement(language,usingNamespace,NamespaceMode.Using));
                    }
                }
            }
            else if (language == Language.CSharp)
            {
                foreach (var usingNamespace in usingNamesapceSet)
                {
                    if (!string.IsNullOrEmpty(usingNamespace))
                    {
                        sb.AppendIndentLine(BufferCodeExporter.GetNamespaceStatement(language, usingNamespace, NamespaceMode.Using));
                    }
                }
                sb.AppendLine("");
                sb.AppendIndentLine(namesapceStatement);
                AddLeftBrace(sb, language);
                return true;
            }
            return false;
        }

        //不支持泛型参数类
        public static void ExportClassName(StringBuilder sb, ClassDefine classNode, Language lanaguage)
        {
            string className = classNode.name;
            //注释
            sb.AppendIndentLine("//" + classNode.desc + ExportProtocolConstant.GenerateComment);
            if (lanaguage == Language.Java)
            {
                sb.AppendIndentLine("public class " + className );
            }
            else if (lanaguage == Language.CSharp)
            {
               sb.AppendIndentLine("public class " + className );// + genericStatement);
            }
        }

        public static void ExportClassMember(StringBuilder sb, ClassDefine classNode, Language language)
        {
            //成员变量
            foreach (var fieldNode in classNode.fieldList)
            {
                sb.AppendIndentLine(fieldNode.ExportCode(language, FieldType.ClassField));
            }

            sb.AppendLine("");
            //构造函数
            string constructorName = classNode.name;
            /*int start = constructorName.IndexOf("<");
            int end = constructorName.LastIndexOf(">");
            if (start != -1 && end != -1)
            {
                constructorName = constructorName.Substring(0, start);
            }*/
            sb.AppendIndentLine(string.Format("public {0}()", constructorName));
            AddLeftBrace(sb, language);
            AddRightBrace(sb, language);

        }

        public static void ExportDecodeFunction(StringBuilder sb, ClassDefine classNode, Language language)
        {
            sb.AppendLine("");
            if (language == Language.CSharp)
            {
                if (classNode.exportStatic)
                    sb.AppendIndentLine(string.Format("public static {0} {0}Decode(BufferBuilder bb,{0} value)",classNode.name));
                else
                    sb.AppendIndentLine("public void Decode(BufferBuilder bb)");
            }
            else if (language == Language.Java)
                sb.AppendIndentLine("public void decode(BufferBuilder bb)");
            AddLeftBrace(sb, language);
            //到时集成到ClassDefine里面去
            foreach (var fieldNode in classNode.fieldList)
            {
                string fieldName = fieldNode.name;
                if (classNode.exportStatic)
                    fieldName = "value." + fieldName;
                string functionName = CodeExporterHelper.GetFunctionStatement(language, CodeMode.Decode,
                    fieldNode.decoration, fieldName, true);
                    sb.AppendIndentLine(functionName);
            }
            AddRightBrace(sb, language);
        }

        public static void ExportEncodeFunction(StringBuilder sb, ClassDefine classNode, Language language)
        {
            sb.AppendLine("");
            if (language == Language.CSharp)
            {
                if (classNode.exportStatic)
                    sb.AppendIndentLine(string.Format("public static void {0}Encode(BufferBuilder bb,{0} value)",
                        classNode.name));
                else
                    sb.AppendIndentLine("public void Encode(BufferBuilder bb)");
            }
            else if (language == Language.Java)
                sb.AppendIndentLine("public void encode(BufferBuilder bb)");
            AddLeftBrace(sb, language);
            foreach (var fieldNode in classNode.fieldList)
            {
                string fieldName = fieldNode.name;
                if (classNode.exportStatic)
                    fieldName = "value." + fieldName;
                string functionName = CodeExporterHelper.GetFunctionStatement(language, CodeMode.Encode,
                    fieldNode.decoration, fieldName, true);
                sb.AppendIndentLine(functionName);
            }
            AddRightBrace(sb, language);
        }

        public static void ExportToStringFunction(StringBuilder sb, ClassDefine classNode, Language lanaguage)
        {
            sb.AppendLine("");
            string toString = string.Empty;
            toString += "\"" + classNode.desc + "(" + classNode.name + "):";
            //成员变量
            List<FieldDefine> fieldList = classNode.fieldList;
            foreach (var fieldNode in fieldList)
            {
                toString += fieldNode.desc + "(\"+" + GetDebugString(fieldNode, lanaguage) + " + \")";
            }
            //ToString函数
            if (lanaguage == Language.CSharp)
                toString = "return @" + toString + "\";";
            else if (lanaguage == Language.Java)
                toString = "return " + toString + "\";";
            if (Language.CSharp == lanaguage)
                sb.AppendIndentLine("public override string ToString()");
            else if (lanaguage == Language.Java)
            {
                sb.AppendIndentLine("public String toString()");
            }
            AddLeftBrace(sb, lanaguage);
            sb.AppendIndentLine(toString);
            AddRightBrace(sb, lanaguage);
            
        }

        public static StringBuilder AppendIndentLine(this StringBuilder sb, string line)
        {
            string indentText = new string('\t', indent);
            sb.AppendLine(indentText + line);
            return sb;
        }

        //添加左括号
        public static void AddLeftBrace(StringBuilder sb, Language lanaguage)
        {
            if (lanaguage == Language.Java)
            {
                sb.Insert(sb.Length - 2, " {");
                indent++;
            }
            else if (lanaguage == Language.CSharp)
            {
                sb.AppendIndentLine("{");
                indent++;
            }
        }

        //添加右括号
        public static void AddRightBrace(StringBuilder sb, Language lanaguage)
        {
            indent--;
            sb.AppendIndentLine("}");
        }
        
        //内嵌
        public static StringBuilder AppendBrace(StringBuilder sb, string head, string body)
        {
            sb.AppendLine("");
            sb.AppendIndentLine(head);
            sb.AppendIndentLine("{");
            indent++;
            sb.AppendIndentLine(body);
            indent--;
            sb.AppendIndentLine("}");
            return sb;
        }
    }

}
