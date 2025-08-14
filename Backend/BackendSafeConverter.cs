using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class BackendSafeConverter
{
    public static object ToSafe(object src)
    {
        if (src == null) return null;

        // 1. ScriptableObject ó��
        if (src is ScriptableObject so)
            return FromScriptableObject(so);

        // 2. IEnumerable ó�� (List, Array ��)
        if (src is IEnumerable enumerable && !(src is string))
        {
            var list = new List<object>();
            foreach (var item in enumerable)
                list.Add(ToSafe(item));
            return list;
        }

        // 3. ��ųʸ� ó��
        if (src is IDictionary dict)
        {
            var safeDict = new Dictionary<string, object>();
            foreach (DictionaryEntry kv in dict)
                safeDict[kv.Key.ToString()] = ToSafe(kv.Value);
            return safeDict;
        }

        // 4. ������(����, enum, string) �״��
        return src;
    }

    private static Dictionary<string, object> FromScriptableObject(ScriptableObject so)
    {
        var result = new Dictionary<string, object>
        {
            { "name", so.name } // �� �ʼ�!
        };

        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        foreach (var f in so.GetType().GetFields(flags))
        {
            if (f.IsNotSerialized) continue;                // [NonSerialized]
            if (f.FieldType.IsSubclassOf(typeof(Object))) continue; // UnityEngine.Object�� name�� �ʿ�

            result[f.Name] = ToSafe(f.GetValue(so));
        }
        return result;
    }
}
