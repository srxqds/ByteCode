/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：May 29 2016
// 模块描述：
//----------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ByteBuilder;
using System;
using Utility;
using System.Reflection;
using UnityEditor;

namespace Serialize
{
    //将C#基础数据序列化为二进制
	public class ExportSerializeHelper
	{
		[MenuItem("Test/DoSerizlize")]
		public static void DoTest()
		{
            Debug.Log(typeof(Dictionary<int,string>).Name);
            Debug.Log(typeof(Dictionary<int, List<string>>).Name);
            Debug.Log(typeof(List<int>).Name);
            Debug.Log(typeof(int[]).Name);
            Debug.Log(typeof(int).FullName);

            Debug.Log(CodeExporterHelper.TypeName2CSharpType("Dictionary<int,List<int>>"));
            Debug.Log(CodeExporterHelper.CSharpType2TypeName(typeof(int)));
            Debug.Log(CodeExporterHelper.CSharpType2TypeName(typeof(Dictionary<int,List<int>>)));
            /*byte[] data = new byte[1024];
			BufferBuilder bb = new BufferBuilder (data);
			bb.Put7BitEncodeInt (127);
			Debug.LogError ("PutPosition1 " +bb.Position);
			bb.Put7BitEncodeInt(128);
			Debug.LogError ("PutPosition2 " +bb.Position);
			bb.Put7BitEncodeInt (int.MaxValue);
			Debug.LogError (int.MaxValue + "PutPosition3 " +bb.Position);
			bb.Put7BitEncodeInt (int.MinValue);
			Debug.LogError (int.MinValue + "PutPosition4 " +bb.Position);
			bb.Put7BitEncodeInt (-1);
			Debug.LogError (-1 + "PutPosition5 " +bb.Position);*/

            /*int testInt = 1;
			SerializeObject (bb, testInt, "int");
			Debug.LogError ("Put Position1:" + bb.Position);
			string testString = "begin";
			SerializeObject (bb, testString, "string");

			Debug.LogError ("Put Position2:" + bb.Position);

			int[] testIntArray = new int[]{ 2, 3 };
			SerializeObject (bb, testIntArray, "int[]");  

			int position = bb.Position;
			Debug.LogError (int.MaxValue + " Put Position3:" + bb.Position);
			bb.Put7BitEncodeInt (int.MaxValue);
			Debug.LogError ("Put Position4:" + bb.Position);
			bb.Put7BitEncodeUint (uint.MaxValue);
			Debug.LogError (uint.MaxValue + "Put Position5:" + bb.Position);
			bb.Put7BitEncodeInt (-1);
			Debug.LogError (-1 + "Put Position6:" + bb.Position);*/



            /*List<int[]> testIntArrayList = new List<int[]> ();
			testIntArrayList.Add (testIntArray);
			SerializeObject (bb, testIntArrayList, "List<int[]>");

			List<string> testStringList = new List<string> ();
			testStringList.Add ("Test");
			SerializeObject (bb, testStringList, "List<string>");

		

			Dictionary<int,string> testDictionary = new Dictionary<int, string> ();
			testDictionary.Add (4, "test1");
			SerializeObject (bb, testDictionary, "Dictionary<int,string>");

            BufferBuilder bbCheck = new BufferBuilder (data);
			Debug.LogError(bbCheck.Get7BitEncodeInt () + " GetPostion1: " + bbCheck.Position);
			Debug.LogError(bbCheck.Get7BitEncodeInt () + " GetPostion2: " + bbCheck.Position);
			Debug.LogError(bbCheck.Get7BitEncodeInt () + " GetPostion3: " + bbCheck.Position);
			Debug.LogError(bbCheck.Get7BitEncodeInt () + " GetPostion4: " + bbCheck.Position);
			Debug.LogError(bbCheck.Get7BitEncodeInt () + " GetPostion5: " + bbCheck.Position);*/
            /*Debug.LogError (bbCheck.GetInt ()+ " Postion1:" + bbCheck.Position);
			Debug.LogError (bbCheck.GetString () + " Postion2:" + bbCheck.Position);
			Debug.LogError(bbCheck.GetNullFlag ());
			Debug.LogError (bbCheck.Get7BitEncodeInt () + " Postion:" + bbCheck.Position);
			Debug.LogError(bbCheck.GetInt() + " " + bbCheck.GetInt() + " Position3: " + bbCheck.Position);

			Debug.LogError (bbCheck.Get7BitEncodeInt () + " " + bbCheck.Get7BitEncodeUint () + " " + bb.Get7BitEncodeInt ());*/


        }

        public static int IndexOf(BufferBuilder source,object value,string typeName)
		{
			BufferBuilder dest = new BufferBuilder(1024);
			SerializeObject (dest, value, typeName);
			return -1;
		}

	    public static void SerializeReference(BufferBuilder bb, int start)
	    {
	        
	    }
		// int ; 0
		// int[] ;[1]
		// int[][] ;[[1,2],[3,4]]
		// List<int> ; [1]
		// List<int[]> ;[[1,2],[3,4]]
		// List<int[]>[] ;[[[1,2],[3,4]]]
		// Dictionary<int,string> [1:test,2:string]
	    public static void SerializeObject(BufferBuilder bb, object value)
	    {
	        string typeName = CodeExporterHelper.CSharpType2TypeName(value.GetType());
            SerializeObject(bb,value,typeName);//.DeclaringType))
        }

        public static void SerializeObject(BufferBuilder bb, object value,string typeName)
		{
			List<object> typeArray = CodeExporterHelper.TypeName2NestArray(typeName);
			if (typeArray != null)
			{
				if (typeArray.Count == 3) //List<>[] Dictionary<,>[]
				{
					SerializeArray(bb,value, typeName);
				}
				else if (typeArray.Count == 2) //List<> Dictionary<>
				{
					string element = typeArray[0] as string;
					if (element.Contains(CodeGenConstants.ComplexType[1])) //List
					{
						SerializeList(bb,value, typeName);
					}
					else if (element.Contains(CodeGenConstants.ComplexType[2])) //Dictionary
					{
						SerializeDictionary(bb,value, typeName);
					}
				}
				else if (typeArray.Count == 1)
				{
					if (typeName.Contains(CodeGenConstants.ComplexType[0])) //array
					{
						SerializeArray(bb,value, typeName);
					}
					else
					{
						SerializePrimitive(bb,value, typeName); //primitive type
					}
				}
			}
        }
        
		public static void SerializePrimitive(BufferBuilder bb,object value, string typeName)
		{
			if (Array.IndexOf (CodeGenConstants.BaseType, typeName) != -1) 
			{
				string methodName = "Put" + typeName.UppercaseFirst ();
				Type type = bb.GetType ();
			    BindingFlags bf = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;
                MethodInfo method = type.GetMethod(methodName, bf);
                method.Invoke(bb, new object[] {value});
			} 
			else if (value is IByteEncode)
			{
				if(!bb.PutNullFlag (value))
					(value as IByteEncode).Encode (bb);
			}
		}

		public static void SerializeArray(BufferBuilder bb,object value, string typeName)
		{
			string elementType = CodeExporterHelper.GetArrayElementTypeName (typeName);
			Array array = value as Array;
			if(!bb.PutNullFlag (value))
			{
				int length = array.Length;
				bb.Put7BitEncodeInt (length);
				for (int i = 0; i < length; i++) 
				{
					SerializeObject (bb,array.GetValue (i),elementType);
				}
			}
		}

		public static void SerializeList(BufferBuilder bb,object value, string typeName)
		{

			IList list = value as IList;
			string genericType = CodeExporterHelper.GetListGenericTypeName (typeName);
			if(!bb.PutNullFlag (value))
			{
				int length = list.Count;
				bb.Put7BitEncodeInt (length);
				for (int i = 0; i < length; i++) 
				{
					SerializeObject (bb,list[i],genericType);
				}
			}
		}

		public static void SerializeDictionary(BufferBuilder bb,object value, string typeName)
		{
			IDictionary dictionary = value as IDictionary;
			string[] genericTypes = CodeExporterHelper.GetDictionaryGenericTypeName (typeName);
			if(!bb.PutNullFlag (value))
			{
				int length = dictionary.Count;
				bb.Put7BitEncodeInt (length);
				if (length != 0) 
				{
					foreach(var key in dictionary.Keys)
					{
						SerializeObject (bb, key,genericTypes[0]);
						SerializeObject (bb, dictionary[key],genericTypes[1]);
					}
				}
			}

		}
	}

}
