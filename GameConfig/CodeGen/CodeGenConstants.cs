/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：April 02 2016
// 模块描述：
//----------------------------------------------------------------*/

using System;


public class CodeGenConstants
{

	public const bool Is7BitEncodeSupport = true;

	// <type="List[T]" />
	public const string XmlConfigGenericTypeLeftFlag = "[";
	public const string XmlConfigGenericTypeRightFlag = "]";

	public const string GenericTypeLeftFlag = "<";
	public const string GenericTypeRightFlag = ">";
    //支持的基础类型,不支持泛型参数
    public static string[] BaseType = new string[] {"bool","byte","sbyte","short","ushort","int","uint",
            "long","ulong","float","double","string" };
    public static string[] ComplexType = new string[] { "[]", "List", "Dictionary" };
    public static string[] CustomType = new string[] {"Vector2","Vector3"};
    //C# 
    public static string[] CSharpBuiltInType = new string[] { "System.Boolean", "System.Byte", "System.SByte", "System.Int16", "System.UInt16", "System.Int32", "System.UInt32",
		"System.Int64", "System.UInt64", "System.Single", "System.Double", "System.String"};

	//System.Collections.Generic.List`1[System.Int32]
	public const string CSharpGenericTypeLeftFlag = "[";
	public const string CSharpGenericTypeRightFlag = "]";

    //Java没用 unsigned integer 暂时先不考虑
	public static string[] JavaBuiltInType = new string[] { "Boolean", "Byte", "SByte", "Short", "Short", "Integer", "Integer",
		"Long", "Long", "Float", "Double", "String"};
    public static string[] JavaComplexType = new string[] {"[]","ArrayList","HashMap"};

    //辅助函数导出类，方便不同模块类名自定义
    public static string SerializeHelperClass = "SerializeHelper.";
    //Decode和Encode函数导出类型
    public static ExportMode exportMode = ExportMode.SelfClass;

    public static bool IsPrimitiveType(string type)
    {
        return Array.IndexOf(BaseType, type) != -1;
    }

    public static string GetTypeConvention(string type)
    {
        return string.Empty;
    }

}

