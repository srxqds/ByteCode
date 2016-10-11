/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：May 29 2016
// 模块描述：
//----------------------------------------------------------------*/

using ByteBuilder;
using Utility;

public class CSharpCodeExporter:ICodeExporter
{
	//减少使用泛型
	//http://stackoverflow.com/questions/6069661/does-system-activator-createinstancet-have-performance-issues-big-enough-to-di
	//Test1 - Activator.CreateInstance<T>(): 8542
	//Test2 - new T() 1082
	//Test3 - Delegate 1214
	//Test4 - Generic new() 8759
	//Test5 - Generic activator 9166
	//Test6 - Generic activator with bindings 60772
	//Baseline 322
	//C#导出语句 [] List Dictionary 以及组合模式
	//导出基础函数Putxxx Getxxx,xxx:类型+数量
	public const string PutArrayFunctionFormat = @"
		public static void {0}(BufferBuilder bb,{1} value)
		{{
			if(!bb.PutNullFlag(value))
			{{
				int length = value.Length;
				bb.Put7BitEncodeInt(length);
				for(int i=0;i<length;i++)
				{{
					{2};
				}}
			}}
		}}
";
	public const string GetArrayFunctionFormat = @"
		public static {1} {0}(BufferBuilder bb)
		{{
			if(bb.GetNullFlag())
			{{
				return null;
			}}
			else
			{{
				int length = bb.Get7BitEncodeInt();
				{1} value = new {2};
				for(int i=0;i<length;i++)
				{{
					value[i] = {3};
				}}
				return value;
			}}
		}}
";

	public const string PutListFunctionFormat = @"
		public static void {0}(BufferBuilder bb,{1} value)
		{{
			if(!bb.PutNullFlag(value))
			{{
				int length = value.Count;
				bb.Put7BitEncodeInt(length);
				for(int i=0;i<length;i++)
				{{
					{2};
				}}
			}}
		}}
";
	public const string GetListFunctionFormat = @"
		public static {1} {0}(BufferBuilder bb)
		{{
			if(bb.GetNullFlag())
			{{
				return null;
			}}
			else
			{{
				int length = bb.Get7BitEncodeInt();
				{1} value = new {1}();
				for(int i=0;i<length;i++)
				{{
					value.Add({2});
				}}
				return value;
			}}
		}}
";

	public const string PutDictionaryFunctionFormat = @"
		public static void {0}(BufferBuilder bb,{1} value)
		{{
			if(!bb.PutNullFlag(value))
			{{
				int length = value.Count;
				bb.Put7BitEncodeInt(length);
				foreach ({2} kv in value) 
				{{
					{3};
					{4};
				}}
			}}
		}}
";

	public const string GetDictionaryFunctionFormat = @"
		public static {1} {0}(BufferBuilder bb)
		{{
			if(bb.GetNullFlag())
			{{
				return null;
			}}
			else
			{{
				int length = bb.Get7BitEncodeInt();
				{1} value = new {1}();
				for(int i=0;i<length;i++)
				{{
					value.Add({2},{3});
				}}
                return value;
			}}
            
		}}
";
    
    public const string PutProtocolVoStatementFormat = @"
        public static void {0}(BufferBuilder bb,{1} value) 
        {{
            if(!bb.PutNullFlag(value))
                {2}
        }}
";
    public const string GetProtocolVoStatementFormat = @"
        public static {1} {0}(BufferBuilder bb)
        {{
            if(bb.GetNullFlag())
                return null;
            else 
            {{
                {1} value = new {1}();
                {2}
                return value;
            }}
        }}
";

    public string GetNamespaceStatement(string @namespace, NamespaceMode mode)
    {
        if (string.IsNullOrEmpty(@namespace) && string.IsNullOrEmpty(ExportProtocolConstant.ClientNamespaceBase))
            return string.Empty;
        if (mode != NamespaceMode.Using)
        {
            if (string.IsNullOrEmpty(@namespace))
                @namespace = ExportProtocolConstant.ClientNamespaceBase;
            else
                @namespace = ExportProtocolConstant.ClientNamespaceBase + "." + @namespace;
            @namespace = @namespace.UppercaseWords();
        }

        if (mode == NamespaceMode.Declare)
            return "namespace "+ @namespace;
        else if(mode == NamespaceMode.Using)
            return "using " + @namespace + ";";
        return @namespace;
    }

    public string GetNamingConvention(string name, ConventionType conventionType)
    {
        if (conventionType == ConventionType.Type)
            return name;
        else if (conventionType == ConventionType.Field)
        {
            
        }
        return string.Empty;
    }

    //C#就是默认配置
    public string GetTypeConvention(string typeName)
    {
        return typeName;
        return string.Empty;
    }

    public string GetCodeModeName (CodeMode codeMode)
	{
		if (codeMode == CodeMode.Decode)
			return "Get";
		else if (codeMode == CodeMode.Encode)
			return "Put";
		return string.Empty;
	}

	public string GetByteBufferCode(string typeName,CodeMode codeMode)
	{
		return typeName.UppercaseFirst ();
	}

    public string GetCustomClassFunctionCode(string typeName, string functionName, CodeMode codeMode)
    {
        if (codeMode == CodeMode.Decode)
        {
            string decodeMethod = string.Empty;
            if (CodeGenConstants.exportMode == ExportMode.HelperClass)
                decodeMethod = "value = " + CodeGenConstants.SerializeHelperClass + typeName + "Decode(bb);";
            else if (CodeGenConstants.exportMode == ExportMode.SelfClass)
                decodeMethod = "value.Decode(bb);";
            return string.Format(GetProtocolVoStatementFormat, functionName, typeName, decodeMethod);
        }
        else if (codeMode == CodeMode.Encode)
        {
            string encodeMethod = string.Empty;
            if (CodeGenConstants.exportMode == ExportMode.HelperClass)
                encodeMethod = CodeGenConstants.SerializeHelperClass + typeName + "Encode(bb,value);";
            else if (CodeGenConstants.exportMode == ExportMode.SelfClass)
                encodeMethod = "value.Encode(bb);";
            return string.Format(PutProtocolVoStatementFormat, functionName, typeName, encodeMethod);
        }
        return string.Empty;
    }

    public string GetArrayFunctionCode(string typeName,string functionName,CodeMode codeMode,string nestFunctionName)
	{
		//nestFunctionName = GetCodeModeName (codeMode) + nestFunctionName;
		//functionName = GetCodeModeName (codeMode) + functionName;
		if (codeMode == CodeMode.Decode)
		{
			int firstLeftBracketIndex = typeName.IndexOf("[");
			string newArrayType = typeName.Insert(firstLeftBracketIndex + 1, "length");
			return string.Format(GetArrayFunctionFormat,functionName, typeName, newArrayType, nestFunctionName);
		}
		else if (codeMode == CodeMode.Encode)
		{
			return string.Format(PutArrayFunctionFormat,functionName, typeName, nestFunctionName);
		}
		return string.Empty;
	}

	public string GetListFunctionCode(string typeName,string functionName,CodeMode codeMode,string nestFunctionName)
	{
		//nestFunctionName = GetCodeModeName (codeMode) + nestFunctionName;
		//functionName = GetCodeModeName (codeMode) + functionName;
		if (codeMode == CodeMode.Decode)
			return string.Format(GetListFunctionFormat,functionName, typeName, nestFunctionName);
		else if (codeMode == CodeMode.Encode)
		{
			return string.Format(PutListFunctionFormat,functionName, typeName,  nestFunctionName);
		}
		return string.Empty;
	}

	public string GetDictionaryFunctionCode (string typeName, string functionName, CodeMode codeMode, string nestFunctionName1, string nestFunctionName2)
	{
		//nestFunctionName1 = GetCodeModeName (codeMode) + nestFunctionName1;
		//nestFunctionName2 = GetCodeModeName (codeMode) + nestFunctionName2;
		//functionName = GetCodeModeName (codeMode) + functionName;
		if(codeMode == CodeMode.Decode)
			return string.Format(GetDictionaryFunctionFormat,functionName, typeName,nestFunctionName1,nestFunctionName2);
		else if (codeMode == CodeMode.Encode)
		{
			int index = typeName.IndexOf (CodeGenConstants.ComplexType [2]);
			string keyValuePairType = typeName.Remove(index, CodeGenConstants.ComplexType[2].Length).Insert(index,"KeyValuePair");
			return string.Format(PutDictionaryFunctionFormat,functionName, typeName, keyValuePairType, nestFunctionName1,nestFunctionName2);
		}
		return string.Empty;
	}

}
