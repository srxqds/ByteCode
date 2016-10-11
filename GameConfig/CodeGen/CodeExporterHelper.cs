/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：March 01 2016
// 模块描述：
//----------------------------------------------------------------*/

using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using Utility;

//实现类型，字符串，序列化函数，中间嵌套数值的相互转换
public class CodeExporterHelper
{
    
    [MenuItem("Test/DoTest")]
    public static void DoTest()
    {
        Debug.LogError(ExtensionUtility.ToString(StringUtility.NestStringArrayParse(
            "[[2001005,1,50],[2001006,1,50],[2001007,1,50],[2001004,1,50],[2001001,1,90],[2001002,1,90],[2001003,1,90]]",
            '[', ']', ',')));
        Debug.LogError(ExtensionUtility.ToString(StringUtility.NestStringArrayParse(
            "[[2001005,1,50]100]",
            '[', ']', ',')));
        Debug.LogError(ExtensionUtility.ToString(StringUtility.NestStringArrayParse(
            "[2001005,1,50]100",
            '[', ']', ',')));
        Debug.LogError(ExtensionUtility.ToString(StringUtility.NestStringArrayParse(
            "101[2001005,1,50]",
            '[', ']', ',')));
        Debug.LogError(ExtensionUtility.ToString(StringUtility.NestStringArrayParse("<<K,V>k1>", '<', '>', ',')));

        Debug.LogError(ExtensionUtility.ToString(StringUtility.NestStringArrayParse("<K2,<K,V>>", '<', '>', ',')));
        Debug.LogError(GetFunctionNameCode("int[]"));
        Debug.LogError(GetFunctionNameCode("int[][]"));
        Debug.LogError(GetFunctionNameCode("List<int[]>"));
        Debug.LogError(GetFunctionNameCode("List<int[]>[]"));
        Debug.LogError(GetFunctionNameCode("Dictionary<int,string>"));
        Debug.LogError(GetFunctionNameCode("Dictionary<int,Dictionary<Dictionary<int,string>,Dictionary<int,string>>>"));
        
        
    }

    //System.Int32 => int，只支持基本类型,List,Dictionary
    public static string CSharpType2TypeName(Type type)
    {
        if(type.HasElementType)
            return CSharpType2TypeName(type.GetElementType()) + "[]";
        else if (type.IsGenericType)
        {
            Type[] arguments = type.GetGenericArguments();
            if (arguments.Length == 1)
                return "List<" + CSharpType2TypeName(arguments[0]) + ">";
            if (arguments.Length == 2)
                return "Dictionary<" + CSharpType2TypeName(arguments[0]) + "," + CSharpType2TypeName(arguments[1]) + ">";

        }
        string typeName = type.ToString();
        //替换类型用于反射生成对象
        for (int i = 0; i < CodeGenConstants.BaseType.Length; i++)
        {
            typeName = typeName.Replace(CodeGenConstants.CSharpBuiltInType[i],CodeGenConstants.BaseType[i]);
        }
        return typeName;
    }

    //int => System.Int32，只支持基本类型,List,Dictionary
    public static Type TypeName2CSharpType(string typeName)
    {
        //替换类型用于反射生成对象
        for (int i = 0; i < CodeGenConstants.BaseType.Length; i++)
        {
            typeName = typeName.Replace(CodeGenConstants.BaseType[i], CodeGenConstants.CSharpBuiltInType[i]);
        }
        for (int i = 1; i < CodeGenConstants.ComplexType.Length; i++)
        {
            typeName = typeName.Replace(CodeGenConstants.ComplexType[i],
                "System.Collections.Generic." + CodeGenConstants.ComplexType[i] + "`" + i);
        }
        //泛型
        typeName = typeName.Replace("<", "[").Replace(">", "]");
        return Type.GetType(typeName);
    }
    
    //int[][] Int2
    //List<int[]> ListInt1
    //Dictionary<int,string> DictionaryInt1String1
    //Dictionary<int,Dictionary<int,string>>
    //Dictionary<List<Dictionary<int,string>>,int>
    //Not support: int[,]
    private static string GetFunctionNameCode(string type)
    {
        Debug.LogError(type);
        List<object> nestArray = StringUtility.NestStringArrayParse(type, '<', '>', ',', true) as List<object>;
        Debug.LogError(ExtensionUtility.ToString(nestArray));
		string functionName = NestArray2FunctionName(Language.CSharp,nestArray,CodeMode.Encode);
		Debug.LogError(GenerateFunctionCode(Language.CSharp,type, CodeMode.Encode));
        return functionName;
    }

	public static List<object> TypeName2NestArray(string type)
	{
		return StringUtility.NestStringArrayParse(type, '<', '>', ',', true) as List<object>;
	}

	public static string TypeName2FunctionName(Language language, string type)
	{
		List<object> nestArray = StringUtility.NestStringArrayParse(type, '<', '>', ',', true) as List<object>;
		return NestArray2FunctionName(language, nestArray);
	}

	public static List<object> FunctionName2NestArray(string functionName)
	{
		return TypeName2NestArray(FunctionName2TypeName(functionName));
	}

	public static string FunctionName2TypeName(string functionName)
	{
		string type = string.Empty;
		int listLength = CodeGenConstants.ComplexType [1].Length;
		int dictionaryLength = CodeGenConstants.ComplexType[2].Length;
		int flag = 0;//1.List 2.DictionaryKey,3.DictionaryValue,4.Array
		Stack<int> flagStack = new Stack<int> ();
		int length = functionName.Length;
		for (int i = 0; i < length; i++) 
		{
			char iter = functionName [i];
			if (char.IsNumber(iter)) 
			{
				int array = Convert.ToInt32 (iter)-48;
				for (int arr = 0; arr < array; arr++)
					type += CodeGenConstants.ComplexType[0];
				if (flagStack.Count > 0 ) 
				{
					flag = flagStack.Pop ();
					if (flag == 4 && flagStack.Count > 0 )
						flag = flagStack.Pop ();
					if (flag == 2) 
					{
						type += ",";
						flagStack.Push (3);

					} 
					else if (flag == 1 || flag == 3) 
					{
						type += ">";

					}
					else if (flag == 4) 
					{
					}
				}
			} 
			else if (i + listLength < length && functionName.Substring (i, listLength) == CodeGenConstants.ComplexType[1]) { //List//List
				type += CodeGenConstants.ComplexType[1];
				type += "<";
				i += listLength-1;
				flagStack.Push (1);
			} else if (i + dictionaryLength < length && functionName.Substring (i, dictionaryLength) == CodeGenConstants.ComplexType[2]) { //Dictionary
				type += CodeGenConstants.ComplexType[2];
				type += "<";
				i += dictionaryLength-1;
				flagStack.Push (2);
			} 
			else 
			{
				for (int j = i; j < length; j++) 
				{
					iter = functionName [j];
					if (char.IsNumber(iter)) 
					{
						type += functionName.Substring (i, j - i).ToLower();
						flagStack.Push (4);
						i = j-1;
						break;
					}
				}
			}
		}
		return type;
	}

	public static string GetArrayElementTypeName(string typeName)
	{
		int lastIndex = typeName.LastIndexOf(CodeGenConstants.ComplexType[0]);
		if (lastIndex == -1 ||
            lastIndex != typeName.Length - CodeGenConstants.ComplexType [0].Length)//error
			return string.Empty;
		return typeName.Remove(lastIndex);
	}

	public static string GetListGenericTypeName(string typeName)
	{
		List<object> typeArray = TypeName2NestArray (typeName);
		string element = typeArray[0] as string;
		if (typeArray.GetCount() < 2 || element != CodeGenConstants.ComplexType[1]) 
			return string.Empty;
		return NestArray2TypeName (typeArray [1] as List<object>);
		
	}

	public static string[] GetDictionaryGenericTypeName(string typeName)
	{
		List<object> typeArray = TypeName2NestArray(typeName);
        string element = typeArray[0] as string;
	    if (typeArray.GetCount() < 2 || element != CodeGenConstants.ComplexType[2])
	        return null;
		List<object> generics = typeArray[1] as List<object>;
		List<string> genericTypes = new List<string>();
		//是否有复杂泛型
		for (int j = 0; j < generics.Count; j++)
		{
			List<object> genericItertor = new List<object>();
			genericItertor.Add(generics[j]);
			if (j + 1 < generics.Count && generics[j + 1] is List<object>)
			{
				j++;
				genericItertor.Add(generics[j]);
			}
			genericTypes.Add(NestArray2TypeName(genericItertor));
		}
		return genericTypes.ToArray ();
	}

	public static string NestArray2TypeName(List<object> nestArray)
	{
		string typeName = string.Empty;
		object element;
		for (int i = 0; i < nestArray.Count; i++)
		{
			element = nestArray[i];
			if (element is string)
			{
				string prefixType = element as string;

				typeName += prefixType;
				if (i + 1 < nestArray.Count && nestArray[i + 1] is List<object>)
				{
					List<object> generics = nestArray[i+1] as List<object>;
					bool hasGeneric = false;
					for (int j = 0; j < generics.Count; j++)
					{
						List<object> genericItertor = new List<object>();
						genericItertor.Add(generics[j]);
						if (j + 1 < generics.Count && generics[j+1] is List<object>)
						{
							j++;
							genericItertor.Add(generics[j]);
						}
						string genericName = NestArray2TypeName (genericItertor);
						if (hasGeneric)
							typeName += "," + genericName + ">";
						else if (j < generics.Count - 1)
							typeName += "<" + genericName;
						else
							typeName += "<" + genericName + ">";
						hasGeneric = true;
					}
					i++;
				}
			}
		}
		return typeName;
	}

	public static string NestArray2FunctionName(Language language,List<object> nestArray, CodeMode codeMode = CodeMode.None)
    {
        string code = string.Empty;
		code += BufferCodeExporter.GetCodeModeName(language,codeMode);
        object element;
        for (int i = 0; i < nestArray.Count; i++)
        {
            element = nestArray[i];
            if (element is string)
            {
                string prefixType = element as string;
                int array = GetArraySuffix(ref prefixType);
				string typeConvention = BufferCodeExporter.GetByteBufferCode(language, prefixType,codeMode);//UppercaseWords();
                
                code += typeConvention;
                if (i + 1 < nestArray.Count && nestArray[i + 1] is List<object>)
                {
                    List<object> generics = nestArray[i + 1] as List<object>;
                    code += NestArray2FunctionName(language,generics,CodeMode.None);
                    i++;
                }
                if (nestArray.Count > 1 || array > 0)
                {
                    code += array;
                }
            }
        }
        return code;
    }
    
    //复杂类型函数导出
    public static string GenerateFunctionCode(Language language,string type,CodeMode codeMode)
    {
        List<object> nestArray = TypeName2NestArray(type);
        string code = string.Empty;
        object element = nestArray[0];
        string functionName = NestArray2FunctionName(language, nestArray,codeMode);
        if (element is string)
        {
            string prefixType = element as string;
            string typeForGetArrayDemision = type;
            int array = GetArraySuffix(ref prefixType);
            if (array == 0)
                array = GetArraySuffix(ref typeForGetArrayDemision);
            if (array > 0) //Array
            {
                string nestStatement = GetFunctionStatement(language, codeMode, GetArrayElementTypeName(type), "value[i]");
                return BufferCodeExporter.GetArrayFunctionCode (language,type, functionName, codeMode, nestStatement);
            }
            else if (prefixType == CodeGenConstants.ComplexType[1])//List
            {
                string nestStatement = GetFunctionStatement(language, codeMode, GetListGenericTypeName(type), "value[i]");
                return BufferCodeExporter.GetListFunctionCode (language,type, functionName, codeMode, nestStatement);
            }
            else if (prefixType == CodeGenConstants.ComplexType[2])//Dictionary
            {
                string[] genericTypes = GetDictionaryGenericTypeName(type);
                string nestStatement1 = GetFunctionStatement(language, codeMode, genericTypes[0], "kv.Key");
                string nestStatement2 = GetFunctionStatement(language, codeMode, genericTypes[1], "kv.Value");

                return BufferCodeExporter.GetDictionaryFunctionCode (language,type, functionName, codeMode, nestStatement1, nestStatement2);
            }
            else if(Array.IndexOf(CodeGenConstants.BaseType, prefixType) == -1) //custom class
            {
                return BufferCodeExporter.GetCustomClassFunctionCode(language,type,functionName,codeMode);
            }
        }
        return code;
    }

    public static int GetArraySuffix(ref string type)
    {
        int arrayDimension = 0;
        int index = type.LastIndexOf("[]");
        while (index == type.Length - 2 && index != -1)
        {
            type = type.Substring(0, index);
            index = type.LastIndexOf("[]");
            arrayDimension += 1;
        }
        return arrayDimension;
    }
    
    //函数调用语句
    public static string GetFunctionStatement(Language language, CodeMode codeMode, string type, string fieldName,bool isStatement = false)
    {
        List<object> nestArray = TypeName2NestArray(type);
        string code = string.Empty;
        object element = nestArray[0];
        string functionName = NestArray2FunctionName(language, nestArray, codeMode);
        if (element is string)
        {
            string prefixType = element as string;
            string typeForGetArrayDemision = type;
            int array = GetArraySuffix(ref prefixType);
            if (array == 0)
                array = GetArraySuffix(ref typeForGetArrayDemision);
            if (Array.IndexOf(CodeGenConstants.BaseType, prefixType) != -1 && array == 0)
            {
                if (CodeMode.Encode == codeMode)
                    code = "bb." + functionName + "(" + fieldName + ")";
                else if (CodeMode.Decode == codeMode)
                    code = "bb." + functionName + "()";
            }
            else
            {
                if (CodeMode.Encode == codeMode)
                    code = CodeGenConstants.SerializeHelperClass + functionName + "(bb," + fieldName + ")";
                else if (CodeMode.Decode == codeMode)
                    code = CodeGenConstants.SerializeHelperClass + functionName + "(bb)";
            }
        }
        if (isStatement)
        {
            if (CodeMode.Decode == codeMode)
                code = fieldName + " = " + code;
            code += ";";
        }
        return code;
    }
    
}
