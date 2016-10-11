/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameDataEditor
{
	public static class JsonHelper
	{
		private const string INDENT_STRING = "    ";
		public static string FormatJson(string str)
		{
			var indent = 0;
			var quoted = false;
			var sb = new StringBuilder();
			for (var i = 0; i < str.Length; i++)
			{
				var ch = str[i];
				switch (ch)
				{
				case '{':
				case '[':
					sb.Append(ch);
					if (!quoted)
					{
						sb.AppendLine();
						Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
					}
					break;
				case '}':
				case ']':
					if (!quoted)
					{
						sb.AppendLine();
						Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
					}
					sb.Append(ch);
					break;
				case '"':
					sb.Append(ch);
					bool escaped = false;
					var index = i;
					while (index > 0 && str[--index] == '\\')
						escaped = !escaped;
					if (!escaped)
						quoted = !quoted;
					break;
				case ',':
					sb.Append(ch);
					if (!quoted)
					{
						sb.AppendLine();
						Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
					}
					break;
				case ':':
					sb.Append(ch);
					if (!quoted)
						sb.Append(" ");
					break;
				default:
					sb.Append(ch);
					break;
				}
			}
			return sb.ToString();
		}
	}

	static class Extensions
	{
		public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
		{
			foreach (var i in ie)
			{
				action(i);
			}
		}
	}
}
