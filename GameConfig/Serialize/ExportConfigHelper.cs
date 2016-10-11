/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：May 18 2016
// 模块描述：
//----------------------------------------------------------------*/

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Utility;

namespace Serialize
{
    //将Excel数据转换为C#对象
    public class ExportConfigHelper
    {
        //todo 1.跨平台支持？ 2.异常数据格式检测 3.加密字段支持 4.重用属性 5.扩展类型支持（Vector3 Enum 等）
        //Dictionary Key:Value
        [MenuItem("Test/DoString2Object")]
        public static void DoTest()
        {
            Debug.LogError(ExtensionUtility.ToString(String2Object("int", "0")));
            Debug.LogError(ExtensionUtility.ToString(String2Object("int[]", "[[1,2]]")));
            /*Debug.LogError(ExtensionUtility.ToString(String2Object("int[][]", "[[1,2],[3,4]]")));
            Debug.LogError(ExtensionUtility.ToString(String2Object("List<int>", "[5,6]")));
            Debug.LogError(ExtensionUtility.ToString(String2Object("List<int[]>[]", "[[[7,8],[9,10]]]")));
            Debug.LogError(ExtensionUtility.ToString(String2Object("Dictionary<int,string>", "[1:test,2:string]")));
            Debug.LogError(ExtensionUtility.ToString(String2Object("Dictionary<int,List<int>>", "[1:[1,2],2:[3,4]]")));
            Debug.LogError(
                ExtensionUtility.ToString(String2Object("Dictionary<int,List<int>>[]",
                    "[[1:[1,2],2:[3,4]],[1:[1,2],2:[3,4]]]")));*/
        }

        public static object String2Object(string type, string value)
        {
            object valueArray = null;
            //字符串里面有[],就直接
            if (type.ToLower().Equals("string") && !string.IsNullOrEmpty(value))
            {
                var list = new List<object>();
                list.Add(value);
                valueArray = list;
            }
            else
            {
                valueArray = StringUtility.NestStringArrayParse(value,
                    ExportConfigConstant.ArrayDataLeftSeparator,
                    ExportConfigConstant.ArrayDataRightSeparator,
                    ExportConfigConstant.ArrayDataElementSeparator);
            }

            object typeArray = StringUtility.NestStringArrayParse(type,
                ExportConfigConstant.GenericTypeLeftSeparator,
                ExportConfigConstant.GenericTypeRightSeparator,
                ExportConfigConstant.GenericTypeMiddleSeparator);
            return String2Value(valueArray, typeArray);
        }

        // int ; 0
        // int[] ;[1]
        // int[][] ;[[1,2],[3,4]]
        // List<int> ; [1]
        // List<int[]> ;[[1,2],[3,4]]
        // List<int[]>[] ;[[[1,2],[3,4]]]
        // Dictionary<int,string> [1:test,2:string]
        public static object String2Value(object value, object type)
        {
            List<object> typeArray = type as List<object>;
            List<object> valueArray = value as List<object>;
            string typeName = CodeExporterHelper.NestArray2TypeName(typeArray);

            if (typeArray != null)
            {
                if (typeArray.Count == 3) //List<>[] Dictionary<,>[]
                {
                    return String2Array(valueArray, typeName);
                }
                else if (typeArray.Count == 2) //List<> Dictionary<>
                {
                    string element = typeArray[0] as string;
                    if (element.Contains(CodeGenConstants.ComplexType[1])) //List
                    {
                        return String2List(valueArray, typeName);
                    }
                    else if (element.Contains(CodeGenConstants.ComplexType[2])) //Dictionary
                    {
                        return String2Dictionary(valueArray, typeName);
                    }
                }
                else if (typeArray.Count == 1)
                {
                    if (typeName.Contains(CodeGenConstants.ComplexType[0])) //array
                    {
                        return String2Array(valueArray, typeName);
                    }
                    else
                    {
                        //primitive type
                        if (valueArray != null)
                        {
                            if (valueArray.Count > 1)
                            {
                                int index = Array.IndexOf(CodeGenConstants.CustomType, typeName);
                                if (index == -1)
                                    throw new Exception("配置错误");
                                else
                                    String2CustomType(valueArray, typeName);
                            }
                            else
                                return String2Primitive(value as string, typeName);
                        }
                       
                    }
                }
            }
            return null;
        }

        public static object String2CustomType(List<object> value, string typeName)
        {
            return null;
        }

        public static object String2Primitive(string value, string typeName)
        {
            if (typeName == CodeGenConstants.BaseType[0])
            {
                if (string.IsNullOrEmpty(value) || value == "0")
                    value = "False";
                else
                    value = "True";
            }
            int index = Array.IndexOf(CodeGenConstants.BaseType, typeName);
            if (index != -1)
            {
                if (index == CodeGenConstants.BaseType.Length - 1) //string
                {
                    return value;
                }
                else //
                {
                    Type targetType = CodeExporterHelper.TypeName2CSharpType(typeName);
                    //default value
                    if (string.IsNullOrEmpty(value))
                        return Activator.CreateInstance(targetType);
                    return Convert.ChangeType(value, targetType);
                }
            }
            return null;
        }

        public static object String2Array(List<object> value, string typeName)
        {
            if (value == null)
                return null;
            int lastIndex = typeName.LastIndexOf(CodeGenConstants.ComplexType[0]);
            string elementType = typeName.Remove(lastIndex);
            List<object> typeArray = CodeExporterHelper.TypeName2NestArray(elementType);

            Type targetType = CodeExporterHelper.TypeName2CSharpType(elementType);
            Array array = Array.CreateInstance(targetType, value.Count);
            for (int i = 0; i < value.Count; i++)
            {
                array.SetValue(String2Value(value[i], typeArray), i);
            }
            return array;
        }

        public static object String2List(List<object> value, string typeName)
        {
            if (value == null)
                return null;
            Type type = CodeExporterHelper.TypeName2CSharpType(typeName);
            object list = Activator.CreateInstance(type);
            List<object> typeArray = CodeExporterHelper.TypeName2NestArray(typeName);
            for (int i = 0; i < value.Count; i++)
            {
                (list as IList).Add(String2Value(value[i], typeArray[1]));
            }
            return list;
        }

        public static object String2Dictionary(List<object> value, string typeName)
        {
            if (value == null)
                return null;
            Type type = CodeExporterHelper.TypeName2CSharpType(typeName);
            object dictionary = Activator.CreateInstance(type);

            string[] genericTypeNames = CodeExporterHelper.GetDictionaryGenericTypeName(typeName);
            //是否有复杂泛型
            bool hasComplexType = false;

            foreach (var genericType in genericTypeNames)
            {
                if (Array.IndexOf(CodeGenConstants.BaseType, genericType) == -1)
                {
                    hasComplexType = true;
                    break;
                }
            }

            List<object> valueArray = new List<object>();
            for (int i = 0; i < value.Count; i++)
            {
                if (value[i] is string)
                {
                    string[] keyValue = (value[i] as string).Split(ExportConfigConstant.KeyValuePairSeparator);
                    //[key:[1]] 和 [[]:[]] 中 : 会添加空内容,但允许 [1:]空字符串的情况出现
                    foreach (var kv in keyValue)
                    {
                        if (!hasComplexType || !string.IsNullOrEmpty(kv))
                        {
                            valueArray.Add(kv);
                        }
                    }
                }
                if (value[i] is List<object>)
                {
                    valueArray.Add(value[i]);

                }
                if (valueArray.Count == 2)
                {
                    object genericType0 = CodeExporterHelper.TypeName2NestArray(genericTypeNames[0]);
                    object genericType1 = CodeExporterHelper.TypeName2NestArray(genericTypeNames[1]);
                    (dictionary as IDictionary).Add(String2Value(valueArray[0], genericType0),
                        String2Value(valueArray[1], genericType1));
                    valueArray.Clear();
                }
            }
            return dictionary;
        }

        
    }
}