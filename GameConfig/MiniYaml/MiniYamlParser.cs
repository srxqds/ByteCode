/*----------------------------------------------------------------
// Copyright (C) 2016 广州，Lucky Game
//
// 模块名：
// 创建者：D.S. Qiu
// 修改者列表：
// 创建日期：September 01 2016
// 模块描述：
//----------------------------------------------------------------*/


using System.Text;
using System.Text.RegularExpressions;

//-----------------------------------------------

/**
 * Mini YAML-like parser. <br/>
 * <em>This is NOT a YAML-compliant parser</em>, see the caveats below.<br/>
 * This parser only reads the following minimal subset of the YAML spec
 * (see {@code http://yaml.org}):
 * <pre>
 * ---  <-- start of document (mandatory)
 * #    <-- a comment line, ignored.
 *      <-- empty lines are ignored (except in multi-line string literals)
 * key:          <-- starts a key YamlBlock in a mapping YamlBlock (e.g. parent container.)
 * key: literal  <-- literal is not typed and internally kept as a string.
 * key: |        <-- '|' means everything that follows is a multi-type string till
 *                   another key of the same indentation OR LESSER is found.
 * - [entry]     <-- define an element in a sequence (e.g. an array).
 * ...  <-- end of document (mandatory)
 * </pre>
 * When a line "key:\n" is parsed, it creates a key-based entity in the container,
 * initially untyped. The container is then left untyped if nothing is defined later.
 * Otherwise it is transformed into a string literal or a sequence (array) or a map (key/values)
 * depending on the next line.
 * <p/>
 * This implementation does not support anything not mentioned explicitly above, e.g.
 * sequences of sequences, or mappings of mappings, (e.g. { } or [ ]), compact nested mapping,
 * tags or references, folded scalars, etc.
 * <p/>
 * Some obvious caveats:
 * <ul>
 * <li> This is NOT a YAML-compliant parser.
 *      It's a subset at best and claims only anecdotal compatibility with the YAML spec.
 * <li> Accepted line breaks are LF, CR or CR+LF.
 * <li> Parser uses a {@link Reader} interface so it's up to the caller to decide on the
 *      charset.
 * <li> Document directives are ignored. In fact anything before or after the document
 *      markers (--- and ...) are ignored.
 * <li> Only explicit documents are supported so --- and ... are mandatory.
 * <li> It's any error to try to mix a sequence (array) and a key mapping in the same YamlBlock.
 * <li> A key can be anything except whitespace and the colon character (:).
 * <li> No reflection or Java bean support. Readers uses the underlying list/maps to retrieve values.
 * </ul>
 */

namespace MiniYaml.Net
{
    public class MiniYamlParser
    {
        //Java version
        //private static Regex RE_EMPTY_LINE = new Regex("^\\s*(?:#.*)?$");
        //C# version
        private static Regex RE_EMPTY_LINE = new Regex(@"^\s*(?:#.*)?$");
        // Indent whitespace.                                      1=indent
        private static Regex RE_INDENT = new Regex(@"^(\s*)[^\s].*$");

        // A key or sequence (list item). This covers 3 cases:
        // 1- A new sequence  item:  ^ - optional_literal
        // 2- A new key:value item:  ^ key: optional_value
        // 3- A new sequence item containing a new key:value item:
        //                           ^ - key: optional_value
        // Option 3 is semantically equivalent to an empty sequence item followed by a key:value.
        //               1=indent  2=seq 3=map key         4=literal (optional)
        private static Regex RE_SEQ_OR_KEY = new Regex(@"^(\s*)(?:(-)|([^\s:]+)\s*:)\s*(.*)$");
        //               1=indent 2=seq 3=map key         4=literal (optional)
        private static Regex RE_SEQ_AND_KEY = new Regex(@"^(\s*)(-)\s*([^\s:]+)\s*:\s*(.*)$");

        public class Input
        {
            private string mUnreadLine = null;
            private int mLineCount = 0;
            private string[] lines;
            private int index;
            public Input(string text)
            {
                text = text.Replace("\r\n", "\n");
                lines = text.Split('\n');
                index = 0;
            }

            public string readLine1()
            {
                string line = null;
                if (index < lines.Length)
                {
                    line = lines[index];
                    index++;
                }

                return line;
            }
            /** Returns a "clean" document line, ignoring empty and comment lines. */
            public string readLine() 
            {
                string line = mUnreadLine;
                if (line != null)
                {
                    mUnreadLine = null;
                    if (!RE_EMPTY_LINE.Match(line).Success)
                    {
                        return line;
                    }
                }

                while ((line = readLine1()) != null)
                {
                    mLineCount++;
                    if (!RE_EMPTY_LINE.Match(line).Success)
                    {
                        break;
                    }
                }

                return line;
            }

            /** Returns a literal line, including empty and comment lines. */
            public string readLiteralLine() 
            {
                string line = mUnreadLine;
                if (line != null) {
                    mUnreadLine = null;
                    return line;
                }

                line = readLine1();
                if (line != null) {
                    mLineCount++;
                }
                return line;
            }

            public int getLineCount()
            {
                return mLineCount;
            }

            public void unreadLine(string line) 
            {
                if (mUnreadLine != null)
                {
                    throw new YamlException("Internal Error: can't only unread 1 line");
                }
                mUnreadLine = line;
            }
        }

        public static YamlBlock parse(string text)
        { 
            Input input = new Input(text);

            // Skip lines till we match the beginning of a document.
            string line;
            while ((line = input.readLine()) != null)
            {
                if ("---".Equals(line))
                {
                    return parseDocument(input);
                }
              
            }

            throw new YamlException(input,
                            "Document marker not found (aka c-directives-end). " +
                            "Tip: start your document with '---'.");
        
        }

        private static YamlBlock parseDocument(Input input)
        {
            YamlBlock doc = new YamlBlock();

            string indent = "";

            string line = input.readLine();
            input.unreadLine(line);
            MatchCollection m = RE_INDENT.Matches(line);
            if (m.Count > 1)
            {
                indent = m[1].Value;
            }

            parseIntoContainer(input, doc, indent);

            line = input.readLine();
            if (!"...".Equals(line))
            {
                    // end of document marker NOT reached.
                    throw new YamlException(input,
                                    "Document end marker not found (aka c-document-end). " +
                                    "Tip: end your document with '...' or check indentation levels.");
            }

            return doc;
        }

        private static void parseIntoContainer(Input input, YamlBlock block, string indent)
        {
            try
            {
                string line;
                while ((line = input.readLine()) != null)
                {
                    if ("...".Equals(line))
                    {
                        // end of document marker reached.
                        input.unreadLine(line);
                        return;
                    }

                    MatchCollection m = RE_SEQ_AND_KEY.Matches(line);
                    if (m.Count == 0)
                    {
                        m = RE_SEQ_OR_KEY.Matches(line);
                    }
                    if (m.Count > 0)
                    {
                        if (m.Count == 4)
                        {
                            
                        }
                        string i2 = m[1].Value;
                        if (i2.Length > indent.Length)
                        {
                            throw new YamlException(input,
                                        string.Format("Mismatched map indentation, expected %d but was %d'",
                                                    indent.Length, i2.Length));
                        }
                        else if (i2.Length < indent.Length)
                        {
                            input.unreadLine(line);
                            return;
                        }

                        YamlBlock c = new YamlBlock();

                        bool parseLiteral = true;
                        if ("-".Equals(m[2].Value))
                        {
                            // group 2 is the - for a pure sequence item
                            //block.(c);

                            if (m[3].Value != null)
                            {
                                // This is a combo sequence item + new key:value *inside* the
                                // new sequence. We simulate this by handling this as a new
                                // sequence item and then change the line by removing
                                // the - marker and recursively iterate to handle a key:value item.
                                line = line.Substring(0, i2.Length) + ' ' + line.Substring(i2.Length + 1);
                                input.unreadLine(line);
                                parseLiteral = false;
                            }

                        }
                        else if (m[3].Value != null)
                        {
                            // group 3 is the key for a key:value item
                            block.SetKeyValue(m[3].Value, c);

                        }
                        else
                        {
                            // This case should not happen.
                            throw new YamlException(input, "Internal error; unmatched syntax: " + line);
                        }

                        if (parseLiteral)
                        {
                            string value = m[4].Value.Trim();
                            if ("|".Equals(value))
                            {
                                // Parse literal string. The multi-line literal stops when
                                // we encounter a potential key:value or sequence item at the
                                // same or outer scope level.
                                StringBuilder sb = new StringBuilder();
                                while ((line = input.readLine()) != null)
                                {
                                    if ("...".Equals(line))
                                    {
                                        // end of document marker reached.
                                        input.unreadLine(line);
                                        break;
                                    }
                                    MatchCollection m2 = RE_SEQ_OR_KEY.Matches(line);
                                    if (m2.Count > 1 && m2[1].Value.Length <= indent.Length)
                                    {
                                        // potential key:value or sequence item found.
                                        input.unreadLine(line);
                                        break;
                                    }

                                    sb.Append(line).Append('\n');
                                }

                                c.SetLiteral(sb.ToString());

                            }
                            else if (value.Length > 0)
                            {
                                c.SetLiteral(value);
                            }
                        }

                        if (!c.isLiteral() && !c.isMapping() && !c.isSequence() )
                        {
                            line = input.readLine();
                            input.unreadLine(line);
                            MatchCollection m2 = RE_INDENT.Matches(line);
                            if (m2.Count > 1)
                            {
                                i2 = m2[1].Value;
                                if (i2.Length > indent.Length)
                                {
                                    parseIntoContainer(input, c, i2);
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new YamlException(input,
                                        "'key:' or '- sequence' expected, found: " + line);
                    }
                }
            }
            catch (YamlException e)
            {
                
            }
        }

    }
}
