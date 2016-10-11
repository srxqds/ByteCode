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

public class JavaCodeExporter:ICodeExporter
{
    public const string PutArrayFunctionFormat = @"
		public void {0}(BufferBuilder bb,{1} value) {{
			if(!bb.putNullFlag(value)) {{
				int length = value.Length;
				bb.put7BitEncodeInt(length);
				for(int i=0;i<length;i++)
				{{
					{2}(bb,value[i]);
				}}
			}}
		}}
";
    public const string GetArrayFunctionFormat = @"
		public {1} {0}(BufferBuilder bb) {{
			if(!bb.getNullFlag()) {{
				int length = bb.get7BitEncodeInt();
				{1} value = new {2};
				for(int i=0;i<length;i++) {{
					value[i] = {3}(bb);
				}}
				return value;
			}}
            else {{
                return null;
            }}
		}}
";

    public const string PutListFunctionFormat = @"
		public void {0}(BufferBuilder bb,{1} value) {{
			if(!bb.putNullFlag(value)) {{
				int length = value.Count;
				bb.put7BitEncodeInt(length);
				for(int i=0;i<length;i++) {{
					{2}(bb,value[i]);
				}}
			}}
		}}
";
    public const string GetListFunctionFormat = @"
		public {1} {0}(BufferBuilder bb)
		{{
			if(bb.getNullFlag())
			{{
				return null;
			}}
			else
			{{
				int length = bb.get7BitEncodeInt();
				{1} value = new {1}();
				for(int i=0;i<length;i++)
				{{
					value.Add({2}(bb));
				}}
				return value;
			}}
		}}
";

    public const string PutDictionaryFunctionFormat = @"
		public void {0}(BufferBuilder bb,{1} value)
		{{
			if(value == null)
			{{
				bb.putNullFlag();
			}}
			else
			{{
				int length = value.Count;
				bb.put7BitEncodeInt(length);
				foreach ({2} kv in test) 
				{{
					{3}(bb,kv.Key);
					{4}(bb,kv.Value);
				}}
			}}
		}}
";

    public const string GetDictionaryFunctionFormat = @"
		public {1} {0}(BufferBuilder bb)
		{{
			if(bb.getNullFlag())
			{{
				return null;
			}}
			else
			{{
				int length = bb.get7BitEncodeInt();
				{1} value = new {1}();
				for(int i=0;i<length;i++)
				{{
					value.Add({2}(bb),{3}(bb));
				}}
			}}
		}}
";

    //java直接生成代码
    //java 解析嵌套类 语句
    public const string PutProtocolVoStatementFormat = @"
        public void {0}(BufferBuilder bb,{1} value) {{
            if(!bb.getNullFlag())
                value.decode(bb);
        }}
";
    //java直接生成代码
    //java 解析嵌套类 语句
    public const string GetProtocolVoStatementFormat = @"
        public {1} {0}(BufferBuilder bb) {{
            if(bb.getNullFlag())
                return null;
            else {{
                {1} value = new {1}();
                value.decode(bb);
                reuturn value;
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
            @namespace = @namespace.ToLower();
        }

        if (mode == NamespaceMode.Declare)
            return "package " + @namespace + ";";
        else if (mode == NamespaceMode.Using)
            return "import " + @namespace + ";";
        return @namespace;
        
    }

    public string GetNamingConvention(string name, ConventionType conventionType)
    {
        return string.Empty;
    }

    public string GetTypeConvention(string typeName)
    {
        return string.Empty;
    }

    public string GetCodeModeName(CodeMode codeMode)
	{
		return string.Empty;
	}

	public string GetByteBufferCode(string typeName,CodeMode codeMode)
	{
		return string.Empty;
	}

    public string GetCustomClassFunctionCode(string typeName, string functionName, CodeMode codeMode)
    {
        if (codeMode == CodeMode.Decode)
        {
            return string.Format(GetProtocolVoStatementFormat, functionName, typeName);
        }
        else if (codeMode == CodeMode.Encode)
        {
            return string.Format(PutProtocolVoStatementFormat, functionName, typeName);
        }
        return string.Empty;
    }

    public string GetArrayFunctionCode(string typeName, string functionName, CodeMode codeMode, string nestFunctionName)
    {
        if (codeMode == CodeMode.Decode)
        {
            int firstLeftBracketIndex = typeName.IndexOf("[");
            string newArrayType = typeName.Insert(firstLeftBracketIndex + 1, "length");
            return string.Format(GetArrayFunctionFormat, functionName, typeName, newArrayType, nestFunctionName);
        }
        else if (codeMode == CodeMode.Encode)
        {
            return string.Format(PutArrayFunctionFormat, functionName, typeName, nestFunctionName);
        }
        return string.Empty;
    }

    public string GetListFunctionCode(string typeName, string functionName, CodeMode codeMode, string nestFunctionName)
    {
        if (codeMode == CodeMode.Decode)
            return string.Format(GetListFunctionFormat, functionName, typeName, nestFunctionName);
        else if (codeMode == CodeMode.Encode)
        {
            return string.Format(PutListFunctionFormat, functionName, typeName, nestFunctionName);
        }
        return string.Empty;
    }

    public string GetDictionaryFunctionCode(string typeName, string functionName, CodeMode codeMode, string nestFunctionName1, string nestFunctionName2)
    {
        if (codeMode == CodeMode.Decode)
            return string.Format(GetDictionaryFunctionFormat, functionName, typeName, nestFunctionName1, nestFunctionName2);
        else if (codeMode == CodeMode.Encode)
        {
            int index = typeName.IndexOf(CodeGenConstants.ComplexType[2]);
            string keyValuePairType = typeName.Remove(index, CodeGenConstants.ComplexType[2].Length).Insert(index, "KeyValuePair");
            return string.Format(PutDictionaryFunctionFormat, functionName, typeName, keyValuePairType, nestFunctionName1, nestFunctionName2);
        }
        return string.Empty;
    }

}
