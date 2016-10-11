/*----------------------------------------------------------------
// Copyright (C) 2015 广州，Lucky Game
//
// 模块名：
// 创建者：D.S.Qiu
// 修改者列表：
// 创建日期：June 02 2016
// 模块描述：
//----------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using UnityEditor;

public class KMPUtility
{
    [MenuItem("Test/DoKMP")]
    public static void DoTest()
    {
        string source = "dfghhyrtfhdfgasgtrthdffasdfsa";
        string dest = "gasg";
        Debug.LogError(Search(dest.ToCharArray(),source.ToCharArray()));
        source = "dfghgasgfhdfgasgtrthdffasdfsa";
        Debug.LogError(SearchSelf(source.ToCharArray(),12,4,0, source.ToCharArray().Length));
    }
    //我也没有具体看过，不知道是不是kmp算法
    //look for dest inside of source
    public static int Search<T>(T[] dest, T[] source)
    {
        int[] table = BuildTable(dest,0,dest.Length);
        // look for this.W inside of S
        int m = 0;
        int i = 0;
        while (m + i < source.Length)
        {
            if (dest[i].Equals(source[m + i]))
            {
                if (i == dest.Length - 1)
                    return m;
                ++i;
            }
            else
            {
                m = m + i - table[i];
                if (table[i] > -1)
                    i = table[i];
                else
                    i = 0;
            }
        }
        return -1;  // not found
    }
    
    private static int[] BuildTable<T>(T[] dest,int begin,int length)
    {
        if (begin < 0 || dest == null || dest.Length < begin + length)
            return null;
        int[] result = new int[length];
        int pos = begin + 2;
        int cnd = 0;
        result[0] = -1;
        result[1] = 0;
        while (pos < length)
        {
            if (dest[pos - 1].Equals(dest[cnd]))
            {
                ++cnd; result[pos] = cnd; ++pos;
            }
            else if (cnd > 0)
                cnd = result[cnd];
            else
            {
                result[pos] = 0; ++pos;
            }
        }
        return result;
    }

    public static int SearchSelf<T>(T[] value, int destBegin, int destLength, int sourceBegin, int sourceLength)
    {
        int[] table = BuildTable(value, destBegin, destLength);
        // look for this.W inside of S
        int m = sourceBegin;
        int i = 0;
        int destEnd = destBegin + destLength;
        int sourceEnd = sourceLength + sourceBegin;
        while (m + i < sourceEnd)
        {
            if (value[destBegin + i].Equals(value[sourceBegin + m + i]))
            {
                if (i == destEnd-1)
                    return m;
                ++i;
            }
            else
            {
                m = m + i - table[i];
                if (table[i] > -1)
                    i = table[i];
                else
                    i = 0;
            }
        }
        return -1;  // not found
    }
}
