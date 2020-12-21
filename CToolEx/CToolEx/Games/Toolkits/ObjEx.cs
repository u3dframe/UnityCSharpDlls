namespace Core.Kernel
{
    using System.Collections.Generic;

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

        static public List<object> ToList(params object[] vals)
        {
            if (vals == null || vals.Length <= 0)
                return null;

            List<object> list = new List<object>();
            for (int i = 0; i < vals.Length; i++)
            {
                if (vals[i] != null)
                    list.Add(vals[i]);
            }
            return list;
        }

        static public int NMaxMore(params int[] vals)
        {
            if (vals == null || vals.Length <= 0) return 0;
            int max = vals[0];
            for (int i = 1; i < vals.Length; i++)
            {
                if (max < vals[i])
                {
                    max = vals[i];
                }
            }
            return max;
        }
        static public int NMax(int v1, int v2, int v3)
        {
            return NMaxMore(v1, v2, v3);
        }

        static public int NMax(int v1, int v2, int v3, int v4)
        {
            return NMaxMore(v1, v2, v3, v4);
        }

        static public double ToDecimal(double org, int acc, bool isRound)
        {
            double pow = 1;
            for (int i = 0; i < acc; i++)
            {
                pow *= 10;
            }

            double temp = org * pow;
            if (isRound)
            {
                temp += 0.5;
            }

            return ((int)temp) / pow;
        }

        static public float Round(double org, int acc)
        {
            return (float)ToDecimal(org, acc, true);
        }

        static public float Round(float org, int acc)
        {
            return (float)ToDecimal(org, acc, true);
        }

        static public int Str2Int(string str)
        {
            int ret = 0;
            int.TryParse(str, out ret);
            return ret;
        }

        static public long Str2Long(string str)
        {
            long ret = 0;
            long.TryParse(str, out ret);
            return ret;
        }

        static public float Str2Float(string str)
        {
            float ret = 0;
            float.TryParse(str, out ret);
            return ret;
        }
    }
}
