/*----------------------------------------------------------------
// Copyright (C) 2016 广州，Lucky Game
//
// 模块名：
// 创建者：D.S. Qiu
// 修改者列表：
// 创建日期：September 01 2016
// 模块描述：
//----------------------------------------------------------------*/

using System;

namespace MiniYaml.Net
{
    public class YamlException:Exception
    {
        public YamlException(string message):base(message)
        {
            
        }

        public YamlException(MiniYamlParser.Input input,string message) : base(message)
        {

        }

    }
}
