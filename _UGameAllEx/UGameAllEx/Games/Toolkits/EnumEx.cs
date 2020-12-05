using System;
/// <summary>
/// 类名 : 枚举工具
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2014-09-09 09:33
/// 功能 : 
/// </summary>
public static class EnumEx {

    // 根据当前枚举值，取得该值所对应的枚举Name(就是Key)
    static public string GetKey4EnumVal(Type enumType, object val)
    {
        return Enum.GetName(enumType, val);
    }

    // 判断枚举是否存在
    static public bool IsHas(Type enumType, object val)
    {
        return Enum.IsDefined(enumType, val);
    }

    //  字符串转换为枚举:该字符串可以是key,也可以是val的ToString();
    static public object ToEnum(Type enumType, string val) {
        if(IsHas(enumType,val))
            return Enum.Parse(enumType, val, true);
        return null;
    }

    static public object ToEnum(Type enumType, int val) {
        return Enum.ToObject(enumType, val);
    }

    static public T Str2Enum<T>(Type enumType, string val) {
        object v = ToEnum(enumType, val);
        if(v != null)
            return (T)v;
        return default(T);
    }

    // int转为枚举
    static public T Int2Enum<T>(Type enumType, int val) {
        return (T)ToEnum(enumType, val);
    }
}
