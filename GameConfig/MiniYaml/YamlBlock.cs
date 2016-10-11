/*----------------------------------------------------------------
// Copyright (C) 2016 广州，Lucky Game
//
// 模块名：
// 创建者：D.S. Qiu
// 修改者列表：
// 创建日期：September 01 2016
// 模块描述：
//----------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MiniYaml.Net
{
    public class YamlBlock
    {

        public bool isLiteral()
        {
            return literal != null;
        }

        public bool isMapping()
        {
            return mapping != null;
        }

        public bool isSequence()
        {
            return sequence != null;
        }

        public string literal { get; private set; }
        public Dictionary<string, YamlBlock> mapping { get; private set; }
        public List<YamlBlock> sequence { get; private set; }

        // --- Literal container

        

        public string GetString()
        {
            return literal;
        }
        

        public YamlBlock GetKey(string key)
        {
            return mapping[key];
        }

        /** A shortcut for {@code getMapping().getKey(key).getString()} for string literal values. */
        public string GetKeyString(string key) 
        {
            YamlBlock value = GetKey(key);
            if (value != null)
            {
                if (!value.isLiteral())
                {
                    throw new YamlException(
                                string.Format("Key '{0}' is of type '{1}', not literal",
                                                key, 0));
                }
                return value.GetString();
            }
            return null;
        }

        // --- Mapping container

        public YamlBlock SetLiteral(string literal)
        {
            if (sequence != null)
            {
                throw new YamlException("Block of type 'sequence' can't be converted to type 'literal'");
            }
            else if (mapping != null)
            {
                throw new YamlException("Block of type 'mapping' can't be converted to type 'literal'");
            }

            this.literal = literal;

            return this;
        }

        public YamlBlock SetKeyValue(string key, YamlBlock value) 
        {
            if (literal != null)
            {
                throw new YamlException("Block of type 'literal' can't be converted to type 'mapping'");
            }
            else if (sequence != null)
            {
                throw new YamlException("Block of type 'sequence' can't be converted to type 'mapping'");
            }
            if (mapping == null)
            {
                mapping = new Dictionary<string, YamlBlock>();
            }
            mapping.Add(key, value);

            return this;
        }

        // --- Sequence container

        public YamlBlock AppendToSequence(YamlBlock block) 
        {
            if (literal != null)
            {
                throw new YamlException("Block of type 'literal' can't be converted to type 'sequence'");
            }
            else if (mapping != null)
            {
                throw new YamlException("Block of type 'mapping' can't be converted to type 'sequence'");
            }
            if (sequence == null)
            {
                sequence = new List<YamlBlock>();
            }
            sequence.Add(block);

            return this;
        }







    }
}
