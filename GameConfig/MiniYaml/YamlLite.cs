/*----------------------------------------------------------------
// Copyright (C) 2016 广州，Lucky Game
//
// 模块名：
// 创建者：D.S. Qiu
// 修改者列表：
// 创建日期：September 03 2016
// 模块描述：
//----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;

public class YAMLite
{
    public class YamlNode : Dictionary<string, YamlNode>
    {
        public string leaf = null;
        public YamlNode parent;

        private bool m_Initialized = false;
        
        public static implicit operator bool(YamlNode n)
        {
            return n != null && n.Count != 0 || n.leaf != null;
        }

        public static implicit operator string(YamlNode n)
        {
            if (n.leaf != null)
            {
                return n.leaf;
            }
            else if (n.Count == 0)
            {
                //Null node
                return "~";
            }
            else
            {
                //Object node
                string yaml = "";
                foreach (KeyValuePair<string, YamlNode> entry in n)
                {
                    yaml += entry.Key + ":";
                    if (entry.Value.leaf != null)
                    {
                        yaml += " " + entry.Value + "\n";
                    }
                    else
                    {
                        foreach (string line in entry.Value.ToString().Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries).ToList())
                        {
                            yaml += "\n  " + line;
                        }
                        yaml += "\n";
                    }
                }
                return yaml;
            }
        }

        public static implicit operator YamlNode(string s)
        {
            YamlNode n = new YamlNode();
            n.leaf = s;
            return n;
        }

        public YamlNode Merge(YamlNode n)
        {

            if (this.leaf != null)
            {
                return n;
            }

            foreach (KeyValuePair<string, YamlNode> entry in n)
            {
                if (!this.ContainsKey(entry.Key))
                {
                    this.Add(entry.Key, entry.Value);
                }
                else
                {
                    //Leaf node on either
                    if (this[entry.Key].leaf != null || entry.Value.leaf != null)
                    {
                        this[entry.Key] = entry.Value;

                        //Object node - recurse!
                    }
                    else
                    {
                        this[entry.Key].Merge(entry.Value);
                    }
                }
            }
            return this;
        }

        public new string ToString()
        {
            return (string)this;
        }

        public YamlNode this[int key]
        {
            get
            {
                return this[key.ToString()];
            }
            set
            {
                this[key.ToString()] = value;
            }
        }

        public new YamlNode this[string key]
        {
            get
            {
                YamlNode outYamlNode = new YamlNode();
                TryGetValue(key, out outYamlNode);
                if (outYamlNode == null)
                {
                    return new YamlNode();
                }
                else
                {
                    return outYamlNode;
                }
            }
            set
            {
                m_Initialized = true;
                this.Remove(key);
                this.Add(key, value);
            }
        }
    }

    public static YamlNode Parse(string yaml)
    {
        yaml = yaml.TrimStart('\n');
        if (yaml.StartsWith("---")) { yaml = yaml.Remove(0, 3); }
        yaml = yaml.TrimStart('\n');
        return Parse(ref yaml, 0);
    }
    

    public static YamlNode Parse(ref string yaml, int indentLevel)
    {
        //Detect unindented array
        bool unIndentedArray = false;
        if (yaml[0] == '\n' && yaml.Remove(0, 1).TrimStart(' ')[0] == '-')
        {
            int _indentLevel = yaml.Remove(0, 1).Length - yaml.Remove(0, 1).TrimStart(' ').Length;
            if (_indentLevel + 2 == indentLevel)
            {
                yaml = yaml.Remove(0, 1);
                unIndentedArray = true;
            }
        }

        int arrayCount = 0;
        YamlNode n = new YamlNode();
        while (yaml.Length != 0)
        {
            char c = yaml[0];

            switch (c)
            {
                case ' ':
                    yaml = yaml.TrimStart(' ');
                    break;
                case '\n':
                    yaml = yaml.Remove(0, 1);
                    int _indentLevel = yaml.Length - yaml.TrimStart(' ').Length;
                    if (unIndentedArray) { _indentLevel += 2; }
                    if (_indentLevel != indentLevel)
                    {
                        yaml = "\n" + yaml;
                        return n;
                    }
                    break;
                case '-':
                    yaml = arrayCount++.ToString() + ": " + yaml.Remove(0, 1);
                    break;
                default:
                    string sym = String.Join("", new string[] { new string(yaml.TakeWhile((_c) => _c != ':' && _c != '\n').ToArray())});
                    yaml = yaml.Remove(0, sym.Length);

                    if (yaml.Length > 0 && yaml[0] == ':')
                    {
                        yaml = yaml.Remove(0, 1).TrimStart(' ');

                        n.Add(sym, Parse(ref yaml, indentLevel + (unIndentedArray ? 0 : 2)));
                    }
                    else
                    {
                        n.leaf = sym;
                        return n;
                    }
                    break;
            }
        }
        return n;
    }
}
