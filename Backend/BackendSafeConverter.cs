using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class BackendSafeConverter
{
    public static object ToSafe(object src)
    {
        if (src == null) return null;

        // 1. ScriptableObject 처리
        if (src is ScriptableObject so)
            return FromScriptableObject(so);

        // 2. IEnumerable 처리 (List, Array …)
        if (src is IEnumerable enumerable && !(src is string))
        {
            var list = new List<object>();
            foreach (var item in enumerable)
                list.Add(ToSafe(item));
            return list;
        }

        // 3. 딕셔너리 처리
        if (src is IDictionary dict)
        {
            var safeDict = new Dictionary<string, object>();
            foreach (DictionaryEntry kv in dict)
                safeDict[kv.Key.ToString()] = ToSafe(kv.Value);
            return safeDict;
        }

        // 4. 나머지(값형, enum, string) 그대로
        return src;
    }

    private static Dictionary<string, object> FromScriptableObject(ScriptableObject so)
    {
        var result = new Dictionary<string, object>
        {
            { "name", so.name } // ← 필수!
        };

        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        foreach (var f in so.GetType().GetFields(flags))
        {
            if (f.IsNotSerialized) continue;                // [NonSerialized]
            if (f.FieldType.IsSubclassOf(typeof(Object))) continue; // UnityEngine.Object는 name만 필요

            result[f.Name] = ToSafe(f.GetValue(so));
        }
        return result;
    }
}
