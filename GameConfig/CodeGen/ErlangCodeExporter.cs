/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：May 29 2016
// 模块描述：
//----------------------------------------------------------------*/

public class ErlangCodeExporter :ICodeExporter
{
    public string GetNamespaceStatement(string @namespace, NamespaceMode mode)
    {
        return string.Empty;
    }

    public string GetTypeConvention(string typeName)
    {
        return string.Empty;
    }

    public string GetNamingConvention(string name, ConventionType conventionType)
    {
        return string.Empty;
    }

    public string GetCodeModeName (CodeMode codeMode)
	{
		return string.Empty;
	}

	public string GetByteBufferCode(string typeName,CodeMode codeMode)
	{

		return string.Empty;
	}

    public string GetCustomClassFunctionCode(string typeName, string functionName, CodeMode codeMode)
    {
        return string.Empty;
    }

    public string GetArrayFunctionCode(string typeName,string functionName,CodeMode codeMode,string nestFunctionName)
	{
		return string.Empty;
	}

	public string GetListFunctionCode(string typeName,string functionName,CodeMode codeMode,string nestFunctionName)
	{
		return string.Empty;
	}

	public string GetDictionaryFunctionCode (string typeName, string functionName, CodeMode codeMode, string nestFunctionName1, string nestFunctionName2)
	{
		return string.Empty;
	}
}
