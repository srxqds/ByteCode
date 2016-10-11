/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：May 18 2016
// 模块描述：
//----------------------------------------------------------------*/

namespace Serialize
{
	public enum CompressLevel
	{
		None,
		Normal,//string
		Maximum,//compare byte sequence ?

	}
    //int_ios(7Bit|WeaponType)
    public class ExportConfigConstant
    {
		public const bool IsSerializeCompress = true;
		public const bool Is7BitEncodeInt = true;
        public const bool GenerateSerializeCode = true;
        //Dictionary Key:Value
        public const char KeyValuePairSeparator = ':';

        public const char ArrayDataLeftSeparator = '[';
        public const char ArrayDataRightSeparator = ']';
        public const char ArrayDataElementSeparator = ',';

        public const char GenericTypeLeftSeparator = '<';
        public const char GenericTypeRightSeparator = '>';
        public const char GenericTypeMiddleSeparator = ',';
        
        public const string GlobalDeserializeClassFilePath = "";

        public const string DeserializeHelperClassFilePath = "";

        public const string SerializeFilePath = "";

        public const string ExcelConfigFolderPath = @"Assets/Data/Config";
        public const string ConfigClassOutputPath = @"Assets/Scripts/Config/";
        public const string Excel2ConfigClassFileName = "data.xml";
        public const string SerializeFileName = "ConfigData.xml";
        //m_Excel Special Index Define
		public const int ClientFieldDescRow = 0;
        public const int ClientFieldTypeRow = 1;
        public const int ClientFieldNameRow = 2;

        public const int ServerFieldNameRow = 3;//

        public const int ExcelDataBeginRow = 4;
        public const int ExcelDataBeginColumn = 1;
        
    }
}
