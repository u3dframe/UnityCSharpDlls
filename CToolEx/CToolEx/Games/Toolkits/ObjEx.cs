namespace Core.Kernel
{
    /// <summary>
    /// 类名 : 对象工具
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2016-12-23 15:20
    /// 功能 : 
    /// </summary>
    public class ObjEx
    {
        static public int LensArrs(object[] arrs)
        {
            if (arrs == null)
                return 0;
            return arrs.Length;
        }

        static public bool IsNullOrEmpty(object[] arrs)
        {
            int lens = LensArrs(arrs);
            return lens <= 0;
        }
    }
}
