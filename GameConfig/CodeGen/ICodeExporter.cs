/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：May 29 2016
// 模块描述：
//----------------------------------------------------------------*/

//int
//7Bit:true
//Obsurced:true
//Platform:iOS
//Enum:Type

public enum Language
{
    None,
    CSharp,
    Java,
    Erlang,
    Cpp,
    Lua,
    JavaScript,
    Python,
    Golang,
    Rust,
}

//为了更好的分离，自动导出和自己编写的分离
public enum ExportMode
{
    HelperClass,//Decode和Encode函数是在Helper里面导出，有些类型只会自动导出Decode和Encode函数 SerializeHelper.XXXDecode
    SelfClass,//Decode和Encode在自身类一起导出
}

public enum CodeMode
{
	None,
	Decode,
	Encode,
}

public enum NamespaceMode
{
    None,
    Using,
    Declare,
}

public enum ConventionType
{
    None,
    Type,
    Field,
    Function,
}


public interface ICodeExporter
{
    //Mode:Namespace.Using为全局路径，其他为相对路径
    string GetNamespaceStatement(string @namespace, NamespaceMode mode);
    //变量名，函数名，类名，类型命名规范
    string GetNamingConvention(string name, ConventionType conventionType);
	string GetCodeModeName (CodeMode codeMode);
	string GetByteBufferCode(string typeName,CodeMode codeMode);
    //自定义类型导出
    string GetCustomClassFunctionCode(string typeName, string functionName, CodeMode codeMode);
    //数组导出
	string GetArrayFunctionCode(string typeName,string functionName, CodeMode codeMode,string nestFunctionName);
    //List 导出
	string GetListFunctionCode(string typeName,string functionName,CodeMode codeMode,string nestFunctionName);
    //Dictionary 导出
	string GetDictionaryFunctionCode (string typeName, string functionName, CodeMode codeMode, string nestFunctionName1, string nestFunctionName2);
}
