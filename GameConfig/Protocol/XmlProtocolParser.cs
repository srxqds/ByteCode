/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：February 24 2016
// 模块描述：
//----------------------------------------------------------------*/

using System.IO;
using System.Text;
using UnityEditor;
using ByteBuilder;

public class XmlProtocolParser
{
    
    public static int indent;

    
    [MenuItem("Custom/Protocol/Export")]
    public static void ParseConfigXml()
    {
        //XmlUtility.Begin(protocolXmlAssetPath);
        string xmlContent = File.ReadAllText(EditorUtils.AssetPath2FilePath(ExportProtocolConstant.ProtocolConfigAssetPath));
        XMLNode rootNode = XMLParser.Parse(xmlContent);
        /*XMLNodeList enumNodes = rootNode.GetNodeList("protocol>0>enum_type>0>type");
        foreach (XMLNode enumNode in enumNodes)
        {
            ExportEnumType(enumNode);
        }*/
        
        XMLNodeList classNodes = rootNode.GetNodeList("protocol>0>custom_type>0>type");
        foreach (XMLNode classNode in classNodes)
        {
            ExportClassType(classNode);
        }
        return;
        XMLNodeList sectionNodes = rootNode.GetNodeList("protocol>0>section");
        foreach (XMLNode sectionNode in sectionNodes)
        {
            ExportSectionMsg(sectionNode);
        }
    }

    public static void ExportEnumType(XMLNode node)
    {
        }

    public static void ExportClassType(XMLNode node)
    {
        /*ClassDefine classDefine = new ClassDefine(node);
        string packageFolder = string.Empty;
        if (!string.IsNullOrEmpty(classDefine.decoration))
        {
            string[] packageNames = classDefine.decoration.Split('.');
            foreach (var packageName in packageNames)
            {
                //首字母大写
                string upperPackage = char.ToUpper(packageName[0]) + packageName.Substring(1);
                packageFolder += upperPackage + "/";
            }
        }
        Directory.CreateDirectory(ExportProtocolConstant.ClientOutputDirectory + packageFolder);
        //服务器端，Java小写
        Directory.CreateDirectory(ExportProtocolConstant.ServerOutputDirectory + packageFolder.ToLower());
        //泛型
        string fileName = classDefine.name;
        int start = fileName.IndexOf("<");
        int end = fileName.IndexOf(">");
        if (start != -1 && end != -1)
            fileName = fileName.Substring(0, start);
        
        File.WriteAllText(EditorUtils.ProjectPath + ExportProtocolConstant.ClientOutputDirectory + packageFolder + fileName + ".cs", classDefine.ExportClientCode().ToString());
        File.WriteAllText(EditorUtils.ProjectPath + ExportProtocolConstant.ServerOutputDirectory + packageFolder.ToLower() + fileName + ".java", classDefine.ExportServerCode().ToString());

        return;*/
        }
    
    public static void ExportSectionMsg(XMLNode node)
    {
        
    }

    private static void ExportC2SMethod(StringBuilder sb, XMLNode sectionNode,XMLNode msgNode)
    {
       
    }

    private static void ExportC2SEncode()
    {
        
    }

    


    
}
