using System;
using System.Collections.Generic;
using System.Text;
/// <summary>
/// XXTEA 加密工具
/// Ancher : Canyon / 龚阳辉
/// Create : 2020-02-29 11:28
/// </summary>
namespace Core.Kernel.Cipher
{
    public static class XXTEA
    {
        private static string _strDefaultKey = "gyh_2020", _strCKey = null;
        static long[] _defaultKey = null, customKey = null;
        private static long[] defaultKey
        {
            get
            {
                if (_defaultKey == null)
                {
                    _defaultKey = Encoding.UTF8.GetBytes(_strDefaultKey.PadRight(MIN_LENGTH, SPECIAL_CHAR)).ToLongArray();
                }
                return _defaultKey;
            }
        }

        static public void SetCustKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                _strCKey = null;
                customKey = null;
                return;
            }
            if (key.Equals(_strCKey))
                return;
            _strCKey = key;
            customKey = Encoding.UTF8.GetBytes(key.PadRight(MIN_LENGTH, SPECIAL_CHAR)).ToLongArray();
        }

        private static long[] GetKey()
        {
            if (customKey != null)
            {
                return customKey;
            }
            return defaultKey;
        }

		static public string custKey{ get { return _strCKey; } }

        public static string Encrypt(byte[] data)
        {
            return TEAEncrypt(data.ToLongArray(),GetKey()).ToHexString();
        }

        public static string Encrypt(this string data)
        {
            return Encrypt(Encoding.UTF8.GetBytes(data.PadRight(MIN_LENGTH, SPECIAL_CHAR)));
        }

        public static string Encrypt(this string data, string key)
        {
            SetCustKey(key);
            return Encrypt(data);
        }
        
        public static string Decrypt(this string data)
        {
            if (string.IsNullOrEmpty(data)) { return data; }
            byte[] code = TEADecrypt(
                data.ToLongArray(),
                GetKey()).ToByteArray();
            return Encoding.UTF8.GetString(code, 0, code.Length);
        }

        public static string Decrypt(this string data, string key)
        {
            SetCustKey(key);
            return Decrypt(data);
        }

        private static long[] TEAEncrypt(long[] data, long[] key)
        {
            int n = data.Length;
            if (n < 1) { return data; }

            long z = data[data.Length - 1], y = data[0], sum = 0, e, p, q;
            q = 6 + 52 / n;
            while (q-- > 0)
            {
                sum += DELTA;
                e = (sum >> 2) & 3;
                for (p = 0; p < n - 1; p++)
                {
                    y = data[p + 1];
                    z = data[p] += (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (key[p & 3 ^ e] ^ z);
                }
                y = data[0];
                z = data[n - 1] += (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (key[p & 3 ^ e] ^ z);
            }

            return data;
        }

        private static long[] TEADecrypt(long[] data, long[] key)
        {
            int n = data.Length;
            if (n < 1) { return data; }

            long z = data[data.Length - 1], y = data[0], sum = 0, e, p, q;
            q = 6 + 52 / n;
            sum = q * DELTA;
            while (sum != 0)
            {
                e = (sum >> 2) & 3;
                for (p = n - 1; p > 0; p--)
                {
                    z = data[p - 1];
                    y = data[p] -= (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (key[p & 3 ^ e] ^ z);
                }
                z = data[n - 1];
                y = data[0] -= (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (key[p & 3 ^ e] ^ z);
                sum -= DELTA;
            }

            return data;
        }

        private static long[] ToLongArray(this byte[] data)
        {
            int n = (data.Length % 8 == 0 ? 0 : 1) + data.Length / 8;
            long[] result = new long[n];

            for (int i = 0; i < n - 1; i++)
            {
                result[i] = BitConverter.ToInt64(data, i * 8);
            }

            byte[] buffer = new byte[8];
            Array.Copy(data, (n - 1) * 8, buffer, 0, data.Length - (n - 1) * 8);
            result[n - 1] = BitConverter.ToInt64(buffer, 0);

            return result;
        }
		
        private static byte[] ToByteArray(this long[] data)
        {
            List<byte> result = new List<byte>(data.Length * 8);

            for (int i = 0; i < data.Length; i++)
            {
                result.AddRange(BitConverter.GetBytes(data[i]));
            }

            while (result[result.Count - 1] == SPECIAL_CHAR)
            {
                result.RemoveAt(result.Count - 1);
            }

            return result.ToArray();
        }

        private static string ToHexString(this long[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2").PadLeft(16, '0'));
            }
            return sb.ToString();
        }

        private static long[] ToLongArray(this string data)
        {
            int len = data.Length / 16;
            long[] result = new long[len];
            for (int i = 0; i < len; i++)
            {
                result[i] = Convert.ToInt64(data.Substring(i * 16, 16), 16);
            }
            return result;
        }

        private const long DELTA = 0x9E3779B9;
        private const int MIN_LENGTH = 32;
        private const char SPECIAL_CHAR = '\0';
    }
}