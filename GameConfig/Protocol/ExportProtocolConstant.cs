using UnityEngine;
using System.Collections;

namespace ByteBuilder
{
    public class ExportProtocolConstant
    {

        public const string GenerateComment = @"(工具自动生成，请勿手动修改！）";

        public static string ClientNamespaceBase = "Com.Net.Proto.Params";

        public const string ServerNamespaceBase = "com.game.params";

        public const string ProtocolConfigAssetPath = "Assets/Xml/protocol_new.xml";

        public const string ClientOutputDirectory = "Assets/Scripts/Net/Protocol/Proto/";

        public const string ServerOutputDirectory = "Assets/Editor/Protocol/Test/";

		public const Language ClientExportLanaguage = Language.CSharp;

		public const Language ServerExprotLanaguage = Language.Java;
    }
}
