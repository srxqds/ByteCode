/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：June 01 2016
// 模块描述：
//----------------------------------------------------------------*/

using System.Collections.Generic;
using System.Text;

namespace Serialize
{
    //反序列化函数辅助函数
    public class ExportDeserializeHelper
    {
        //不包括自身type,没有去重
        public static List<string> GetAllNestType(string type)
        {
            List<string> nestTypes = new List<string>();
            string arrayElementType = CodeExporterHelper.GetArrayElementTypeName(type);
            //
            if(!string.IsNullOrEmpty(arrayElementType))
            {
                nestTypes.Add(arrayElementType);
                nestTypes.AddRange(GetAllNestType(arrayElementType));
            }

            string listElementType = CodeExporterHelper.GetListGenericTypeName(type);
            if (!string.IsNullOrEmpty(listElementType))
            {
                nestTypes.Add(listElementType);
                nestTypes.AddRange(GetAllNestType(listElementType));
            }

            string[] dictElementTypes = CodeExporterHelper.GetDictionaryGenericTypeName(type);
            if (dictElementTypes != null)
            {
                nestTypes.Add(dictElementTypes[0]);
                nestTypes.AddRange(GetAllNestType(dictElementTypes[0]));
                nestTypes.Add(dictElementTypes[1]);
                nestTypes.AddRange(GetAllNestType(dictElementTypes[1]));
            }
            return nestTypes;
        }

        //导出类型的序列化辅助函数
        public static string ExportHelperFunctionClass(ClassDefine helperClass, List<string> types,Language language)
        {
            HashSet<string> allItertorType = new HashSet<string>();
            foreach(string type in types)
            {
                if (!allItertorType.Contains(type))
                {
                    allItertorType.Add(type);
                }
                List<string> nestTypes = GetAllNestType(type);
                foreach(string nestType in nestTypes)
                {
                    if (!allItertorType.Contains(nestType))
                        allItertorType.Add(nestType);
                }
                FieldDefine fd = new FieldDefine();
                fd.decoration = type;
                helperClass.fieldList.Add(fd);
            }
            StringBuilder sb = new StringBuilder();
            ExportClassHelper.indent = 0;
            
            bool hasLeftBrace = ExportClassHelper.ExportNamespace(sb, helperClass, language);
            ExportClassHelper.ExportClassName(sb, helperClass, language);
            ExportClassHelper.AddLeftBrace(sb, language);
            //function 
            sb.AppendLine("");
            foreach (var type in allItertorType)
            {
                string function = CodeExporterHelper.GenerateFunctionCode(language, type, CodeMode.Decode);
                if (!string.IsNullOrEmpty(function))
                {
                    sb.AppendIndentLine(function);
                }
                if (ExportConfigConstant.GenerateSerializeCode)
                {
                    function = CodeExporterHelper.GenerateFunctionCode(language, type, CodeMode.Encode);
                    if (!string.IsNullOrEmpty(function))
                    {
                        sb.AppendIndentLine(function);
                    }
                }
            }
            sb.AppendLine("");
            //class end
            ExportClassHelper.AddRightBrace(sb, language);
            //namespace end
            if (hasLeftBrace)
                ExportClassHelper.AddRightBrace(sb, language);
            return sb.ToString();
        }

        public static string ExportConfigDeserializeClass(ClassDefine deserializerClass, List<string> types,
            Language language)
        {
            StringBuilder sb = new StringBuilder();
            ExportClassHelper.indent = 0;
            FieldDefine fd = new FieldDefine();
            fd.decoration = "Dictionary<int,object>";
            deserializerClass.fieldList.Add(fd);
            bool hasLeftBrace = ExportClassHelper.ExportNamespace(sb, deserializerClass, language);
            ExportClassHelper.ExportClassName(sb, deserializerClass, language);
            ExportClassHelper.AddLeftBrace(sb, language);

            sb.AppendLine("");
            if (language == Language.CSharp)
                sb.AppendIndentLine("public static void Deserialize(BufferBuilder bb)");
            else if (language == Language.Java)
            { }
            ExportClassHelper.AddLeftBrace(sb, language);
            foreach (var type in types)
            {
                sb.AppendIndentLine(type + ".LoadTable(bb);");
            }
            ExportClassHelper.AddRightBrace(sb, language);
            //class end
            ExportClassHelper.AddRightBrace(sb, language);
            //namespace end
            if (hasLeftBrace)
                ExportClassHelper.AddRightBrace(sb, language);
            return sb.ToString();
        }
        //旧版本
        public static string ExportDeserializeClass(ClassDefine deserializerClass, List<string> types, Language language)
        {
            StringBuilder sb = new StringBuilder();
            ExportClassHelper.indent = 0;
            FieldDefine fd = new FieldDefine();
            fd.decoration = "Dictionary<int,object>";
            deserializerClass.fieldList.Add(fd);
            bool hasLeftBrace = ExportClassHelper.ExportNamespace(sb, deserializerClass, language);
            ExportClassHelper.ExportClassName(sb, deserializerClass, language);
            ExportClassHelper.AddLeftBrace(sb, language);

            sb.AppendLine("");
            if (language == Language.CSharp)
                sb.AppendIndentLine("public static Dictionary<string,Dictionary<int,object>> Deserialize(BufferBuilder bb)");
            else if (language == Language.Java)
                sb.AppendIndentLine("public Dictionary<string,Dictionary<int,object>> deserialize(BufferBuilder bb)");
            ExportClassHelper.AddLeftBrace(sb, language);
            sb.AppendIndentLine("Dictionary<string,Dictionary<int,object>> valueDicts= new Dictionary<string,Dictionary<int,object>>();");
            sb.AppendIndentLine("Dictionary<int,object> valueDict;");
            foreach (var type in types)
            {
                string elementType = CodeExporterHelper.GetArrayElementTypeName(type);
                sb.AppendIndentLine("valueDict = new Dictionary<int,object>();");
                string valuesName = elementType + "Values";
                string function = CodeExporterHelper.GetFunctionStatement(language, CodeMode.Decode, type, valuesName, true);
                if(!string.IsNullOrEmpty(function))
                    sb.AppendIndentLine(type + " " + function);

                sb.AppendIndentLine("for(int i=0;i<" + valuesName + ".Length;i++)");
                ExportClassHelper.AddLeftBrace(sb, language);
                if(type.Contains("GlobalConfig"))
                    sb.AppendIndentLine("valueDict.Add(i+1," + valuesName + "[i]);");
                else
                    sb.AppendIndentLine("valueDict.Add(" + valuesName + "[i].id," + valuesName + "[i]);");
                ExportClassHelper.AddRightBrace(sb, language);
                sb.AppendIndentLine("valueDicts.Add(\"" + elementType + "\",valueDict);");
            }
            sb.AppendLine("");
            sb.AppendIndentLine("return valueDicts;");
            ExportClassHelper.AddRightBrace(sb, language);
            //class end
            ExportClassHelper.AddRightBrace(sb, language);
            //namespace end
            if (hasLeftBrace)
                ExportClassHelper.AddRightBrace(sb, language);
            return sb.ToString();
        }
        

    }
}
