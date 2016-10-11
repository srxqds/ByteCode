/*----------------------------------------------------------------
// Copyright (C) 2016 广州，Lucky Game
//
// 模块名：
// 创建者：D.S. Qiu
// 修改者列表：
// 创建日期：September 03 2016
// 模块描述：
//----------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace MiniYaml.Net
{
    public class YamlParserTest
    {

        [MenuItem("Test/YamlParserTest")]
        public static void DoTest()
        {
            string testText =
                @"# Project: MiniYamlParser -- test data file -- (c) 2012 ralfoide -- Apache License, Version 2.0.
---
format: 1.0
description: A key/value set used to configure an app of mine. It contains a multi-line script.
items:
  - name: intro
    link:   http://www.example.com/test1
    dpi:    320
    text:
        1: All inner space is    preserved. Rest is trimmed.
        3:          Interested? 
    landscape: |
      resize width 100%
      move image 50% 40% to screen 85% 5%
    portrait: |
      resize height 75%
      move image 50% 0% to screen 10% 5%
    
  - name: family
    link:   http://www.example.com/test2
    dpi:    160
    text:
        1: All your
        2: Bases
        3: make
        4: your time
        5: belong to us.

  - name: sleep
    landscape: | 
        text-color #AAAAAA
        text 1 at  4%   68% size 12%

  - name: work
    portrait: |
        font Serif
        text-color #AAAAAA
...
# end";

            YamlBlock doc = MiniYamlParser.parse(testText);
            Debug.Log(doc);
        }
    }
}
