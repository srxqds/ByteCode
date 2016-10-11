/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：May 29 2016
// 模块描述：
//----------------------------------------------------------------*/

using System.Collections.Generic;


public class BufferCodeExporter
{
	public static Dictionary<Language,ICodeExporter> codeExporters = new Dictionary<Language, ICodeExporter>();

	static BufferCodeExporter()
	{
		codeExporters.Add (Language.CSharp, new CSharpCodeExporter());
		codeExporters.Add (Language.Java, new JavaCodeExporter());
		codeExporters.Add (Language.Erlang, new ErlangCodeExporter());
	}

	public static ICodeExporter GetCodeExporter(Language language)
	{
		if(codeExporters.ContainsKey(language))
			return codeExporters[language];
		return null;
	}

    public static string GetNamespaceStatement(Language language, string @namespace, NamespaceMode mode)
    {
        return GetCodeExporter(language).GetNamespaceStatement(@namespace, mode);
    }

    public static string GetCodeModeName(Language language, CodeMode codeMode)
    {
        return GetCodeExporter(language).GetCodeModeName(codeMode);
    }
    public static string GetByteBufferCode(Language language, string typeName, CodeMode codeMode)
    {
        return GetCodeExporter(language).GetByteBufferCode(typeName, codeMode);
    }

    public static string GetCustomClassFunctionCode(Language language, string typeName, string functionName, CodeMode codeMode)
    {
        return GetCodeExporter(language).GetCustomClassFunctionCode(typeName, functionName, codeMode);
    }

    public static string GetArrayFunctionCode(Language language, string typeName, string functionName, CodeMode codeMode, string nestFunctionName)
    {
        return GetCodeExporter(language).GetArrayFunctionCode(typeName, functionName, codeMode, nestFunctionName);
    }

    public static string GetListFunctionCode(Language language, string typeName, string functionName, CodeMode codeMode, string nestFunctionName)
    {
        return GetCodeExporter(language).GetListFunctionCode(typeName, functionName, codeMode, nestFunctionName);
    }

    public static string GetDictionaryFunctionCode(Language language, string typeName, string functionName, CodeMode codeMode, string nestFunctionName1, string nestFunctionName2)
    {
        return GetCodeExporter(language).GetDictionaryFunctionCode(typeName, functionName, codeMode, nestFunctionName1, nestFunctionName2);
    }

}
