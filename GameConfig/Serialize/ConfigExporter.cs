/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：March 01 2016
// 模块描述：
//----------------------------------------------------------------*/
using System.IO;

public class ConfigExporter
{
    public const string ExcelDirectory = "Assets/Editor/Test/Config/";
    public const string CodeDirectory = "Assets/Scripts/Game/Config/";
    public const string SerializeFilePath = "Assets/Xml/Config.xml";

    public const int FiledDescLine = 1;
    public const int FiledTypeLine = 2;
    public const int ClientFiledLine = 3;
    public const int ServerFiledLine = 4;

    public static XMLNodeList GetConfigDataXml(string xmlPath)
    {
        string xmlContent = File.ReadAllText(xmlPath);
        XMLNode rootNode = XMLParser.Parse(xmlContent);
        if (rootNode != null)
            return rootNode.GetNodeList("items>0>config>0>item");
        return null;
    }
}
