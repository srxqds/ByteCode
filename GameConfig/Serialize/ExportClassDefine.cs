/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：May 26 2016
// 模块描述：
//----------------------------------------------------------------*/
using System.Collections.Generic;
using System.Text;

namespace Serialize
{

    public enum FieldType
    {
        ClassHead,
        ClassField,
        EnumHead,
        EnumField,
        C2SField,
        C2SHead,
        SectionHead,
    }

    public class FieldDefine
    {
        public string name;
        public string desc;//注释
        public string decoration;//类型，命名空间(以|分割)

        public virtual string ExportCode(Language language, FieldType fieldType)
        {
            if (fieldType == FieldType.ClassField)
            {
                var decorationShow = decoration;
                var nameShow = name;
                if (language == Language.Java)
                {
                    decorationShow = decoration.Replace("bool", "boolean");
                    decorationShow = decorationShow.Replace("string", "String");
                    decorationShow = decorationShow.Replace("List<int>", "List<Integer>");
                    decorationShow = decorationShow.Replace("List<long>", "List<Long>");
                    decorationShow = decorationShow.Replace("List<short>", "List<Short>");
                    decorationShow = decorationShow.Replace("List<string>", "List<String>");
                    decorationShow = decorationShow.Replace("List<float>", "List<Float>");
                    decorationShow = decorationShow.Replace("List<double>", "List<Double>");
                }
                else if (language == Language.CSharp)
                {

                }
                //nameShow = ExportProtocolHelper.GetNamingConvention(name, language);
                return "public " + decorationShow + " " + nameShow + ";//" + desc;
            }
            return string.Empty;
        }
    }

    public class ClassDefine : FieldDefine
    {
        public string baseClass;
        public bool exportStatic = false;

        public List<FieldDefine> fieldList = new List<FieldDefine>();

        //Class Layout
        //namespace
        //class 
        //member，constructor
        //function：Encode，Decode,ToString
        public override string ExportCode(Language lanaguage, FieldType fieldType)
        {
            StringBuilder sb = new StringBuilder();
            ExportClassHelper.indent = 0;
            bool hasLeftBrace = ExportClassHelper.ExportNamespace(sb, this, lanaguage);
            ExportClassHelper.ExportClassName(sb, this, lanaguage);
            ExportClassHelper.AddLeftBrace(sb, lanaguage);
            ExportClassHelper.ExportClassMember(sb, this, lanaguage);
            ExportClassHelper.ExportDecodeFunction(sb, this, lanaguage);
            ExportClassHelper.ExportEncodeFunction(sb, this, lanaguage);
            //ExportClassHelper.ExportToStringFunction(sb, this, lanaguage);
            //class end
            ExportClassHelper.AddRightBrace(sb, lanaguage);
            //namespace end
            if (hasLeftBrace)
                ExportClassHelper.AddRightBrace(sb, lanaguage);
            return sb.ToString();
        }
    }

}
