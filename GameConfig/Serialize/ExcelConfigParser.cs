/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：May 27 2016
// 模块描述：
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
//using Com.Net.Proto.Params;
using PureExcel;
using UnityEditor;
using ByteBuilder;
using Debug = UnityEngine.Debug;

namespace Serialize
{
    public class ExcelConfigParser
    {
        [MenuItem("Custom/Config/Deserialize")]
        public static void DoDeserialize()
        {
           
            byte[] data = File.ReadAllBytes(UnityEngine.Application.dataPath + ExportConfigConstant.ConfigClassOutputPath + ExportConfigConstant.SerializeFileName);
            BufferBuilder bb = new BufferBuilder(data);
            Stopwatch st = new Stopwatch();//实例化类
            st.Start();//开始计时
            //需要统计时间的代码段
            //Dictionary<string, Dictionary<int, object>> value = DeserializeHelper.Deserialize(bb);
            st.Stop();//终止计时
            Debug.LogError(st.ElapsedMilliseconds.ToString());//输出时间。输出运行时间：Elapsed，带毫秒的时间：ElapsedMilliseconds
               
        }

        [MenuItem("Custom/Config/Serialize")]
        public static void DoExport()
        {
			string configClassOutputPath = EditorUtils.AssetPath2FilePath(ExportConfigConstant.ConfigClassOutputPath);

			string excelConfigPath = EditorUtils.AssetPath2FilePath(ExportConfigConstant.ExcelConfigFolderPath);

            Directory.CreateDirectory(configClassOutputPath);
            ClassDefine helperClass = new ClassDefine();
            List<string> functionList = new List<string>();
            helperClass.name = "SerializeHelper";
            ClassDefine deserializeClass = new ClassDefine();
            deserializeClass.name = "DeserializeHelper";
            List<string> typeList = new List<string>();
            byte[] data = new byte[1024];
		    BufferBuilder bb = new BufferBuilder (data);

            XMLNodeList nodeList =
                ConfigExporter.GetConfigDataXml(excelConfigPath +
                                                ExportConfigConstant.Excel2ConfigClassFileName);
            int objectTotal = 0;
            string test = string.Empty;
            int count = 0;
            foreach (XMLNode node in nodeList)
            {
                count++;
                string excelFileName = node.GetValue("@file");
                Debug.Log("excelFileName:" + excelFileName + " Application.dataPath:" + UnityEngine.Application.dataPath);
                EditorUtility.DisplayProgressBar("Export Config", "Export " + excelFileName,count/(float)nodeList.Count);
                string clientCsFileName = node.GetValue("@client");
                string excelFilePath = excelConfigPath + excelFileName + ".xlsx";

                
                if (!File.Exists(excelFilePath) || string.IsNullOrEmpty(clientCsFileName))
                {
                    Debug.LogError(excelFilePath + " " + clientCsFileName);
                    continue;
                }
                /*
                if (!clientCsFileName.Contains("SoundConfig"))
                {
                    continue;
                }*/
                Excel excelParser = new Excel(excelFilePath);
                Worksheet workSheet = excelParser.Read(0);
                excelParser.Dispose();
                int rowCount = workSheet.RowCount;
                int columnCount = workSheet.ColumnCount;
                
                ClassDefine classDefine = new ClassDefine();
                classDefine.desc = excelFileName;
                classDefine.name = clientCsFileName;
                string deserializeType = classDefine.name + CodeGenConstants.ComplexType[0];
                typeList.Add(deserializeType);
                functionList.Add(deserializeType);

                int objectCount = rowCount - ExportConfigConstant.ExcelDataBeginRow;
                objectTotal += objectCount;
                test += clientCsFileName + "," + objectCount + "\n";
                bb.PutBool(false);
                if(excelFileName.Contains("q全局配置表"))
                    bb.Put7BitEncodeInt(1);
                else 
                    bb.Put7BitEncodeInt(objectCount);
                Debug.LogError(clientCsFileName + " row: " + rowCount + " column:" + columnCount + " nodeList cnt:" + nodeList.Count);

                if (excelFileName.Contains("q全局配置表"))
                {
                    for (int column = 0; column < columnCount; column++)
                    {
                        if (column != 0)
                            bb.PutBool(false);
                        for (int row = 6; row < rowCount;
                            row++)
                        {
                            string name = workSheet.GetCell(row, 1)
                                .Replace(" ", "");
                            string type = workSheet.GetCell(row, 2)
                                .Replace(" ", "");
                            type = type.Replace("String", "string");
                            //Debug.LogError("column:" + column + " name:" + name + " type:" + type);
                            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(name))
                                continue;
                            if (column == 0) //先读取配置头
                            {
                                FieldDefine fieldDefine = new FieldDefine();
                                fieldDefine.desc =
                                    workSheet.GetCell(row, 0).Trim();
                                    //.ToString();
                                fieldDefine.desc = fieldDefine.desc.Replace("\n", "").Replace("\r", "");
                                fieldDefine.name = name; //.ToString();
                                fieldDefine.decoration = type;
                                classDefine.fieldList.Add(fieldDefine);
                                if (!functionList.Contains(fieldDefine.decoration))
                                    functionList.Add(fieldDefine.decoration);
                            }
                            else //遍历读取数据,序列化保存数据
                            {

                                string value = workSheet.GetCell(row, column);
                                if (value == null)
                                    value = string.Empty;
                                //这里要注释掉
                                value = value.Replace(";", ",").Replace("；", ",");
                                value = value.Replace("，", ",");
                                value = value.Trim();
                                try
                                {
                                    object serializeValue = ExportConfigHelper.String2Object(type, value);
                                    ExportSerializeHelper.SerializeObject(bb, serializeValue, type);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError("cell(" + row + "," + column + "):" + value + " type:" + type);
                                    Debug.LogError(ex);
                                }

                            }
                        }
                        if (column == 0)
                            column = 2;
                    }
                }
                else
                {
                    Debug.Log("rowCount:" + rowCount );
                    for (int row = 0; row < rowCount; row++)
                    {
                        if(row != 0)
                            bb.PutBool(false);
                        for (int column = ExportConfigConstant.ExcelDataBeginColumn;
                            column < columnCount;
                            column++)
                        {
                            
                            string name = workSheet.GetCell(ExportConfigConstant.ClientFieldNameRow, column)
                                .Replace(" ", "");
                            string type = workSheet.GetCell(ExportConfigConstant.ClientFieldTypeRow, column)
                                .Replace(" ", "");
                            type = type.Replace("String", "string");
                            //Debug.LogError("column:" + column + " name:" + name + " type:" + type);
                            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(name))
                                continue;
                            if (row == 0) //先读取配置头
                            {
                                FieldDefine fieldDefine = new FieldDefine();
                                fieldDefine.desc =
                                    workSheet.GetCell(ExportConfigConstant.ClientFieldDescRow, column).Trim();
                                    //.ToString();
                                fieldDefine.desc = fieldDefine.desc.Replace("\n", "").Replace("\r", "");
                                fieldDefine.name = name; //.ToString();
                                fieldDefine.decoration = type;
                                classDefine.fieldList.Add(fieldDefine);
                                if (!functionList.Contains(fieldDefine.decoration))
                                    functionList.Add(fieldDefine.decoration);
                            }
                            else //遍历读取数据,序列化保存数据
                            {

                                string value = workSheet.GetCell(row, column);
                                if (value == null)
                                    value = string.Empty;
                                //这里要注释掉
                                value = value.Replace(";", ",").Replace("；", ",");
                                value = value.Replace(" ", "");
                                value = value.Replace("，", ",");
                                try
                                {

                                    object serializeValue = ExportConfigHelper.String2Object(type, value);
                                    ExportSerializeHelper.SerializeObject(bb, serializeValue, type);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError("cell(" + row + "," + column + "):" + value + " type:" + type);
                                    Debug.LogError(ex);
                                }

                            }
                        }
                        if (row == 0)
                            row = ExportConfigConstant.ExcelDataBeginRow - 1;
                        
                    }
                }
                //导出配置类
                File.WriteAllText(configClassOutputPath + classDefine.name + ".cs", classDefine.ExportCode(Language.CSharp, 0));
                //break;
                //Debug.LogError(classDefine.ExportCode(Language.CSharp, 0));
            }
            EditorUtility.ClearProgressBar();
            Debug.LogError("position:" + bb.Position + " serialize object count:" + objectTotal + " :" + test);
            //辅助函数，全局序列化函数
            string deserializeClassCode = ExportDeserializeHelper.ExportDeserializeClass(deserializeClass, typeList, Language.CSharp);
            string helperClassCode = ExportDeserializeHelper.ExportHelperFunctionClass(helperClass, functionList, Language.CSharp);
            File.WriteAllText(configClassOutputPath + deserializeClass.name + ".cs",deserializeClassCode);
            File.WriteAllText(configClassOutputPath + helperClass.name + ".cs",helperClassCode);
            File.WriteAllBytes(configClassOutputPath + ExportConfigConstant.SerializeFileName , bb.ToArray());
        }
    }

}
