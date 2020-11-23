using UnityEngine;
using System.IO;
using Core.Kernel.Cipher;

namespace Core.Kernel
{
    /// <summary>
    /// 类名 : 文件路径对象父类
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2020-11-15 14:29
    /// 功能 : 
    /// </summary>
    public class UGameFile : UGameRes
    {
        static private readonly UGameFile instance = new UGameFile();
        static private UGameFile _curInstance = instance;
        static public UGameFile curInstance {
            get { return _curInstance; }
            set
            {
                if (value != null)
                    _curInstance = value;
            }
        }

        static public EM_EnCode EncodeWordFile = EM_EnCode.BASE64; // 编码文本文件
        
        // 加密
        static public string Encrypt(string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                switch (EncodeWordFile)
                {
                    case EM_EnCode.XXTEA:
                        val = XXTEA.Encrypt(val);
                        break;
                    case EM_EnCode.BASE64:
                        val = Base64Ex.Encode(val);
                        break;
                }
            }
            return val;
        }

        // 解密
        static public string Decrypt(string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                switch (EncodeWordFile)
                {
                    case EM_EnCode.XXTEA:
                        val = XXTEA.Decrypt(val);
                        break;
                    case EM_EnCode.BASE64:
                        val = Base64Ex.Decode(val);
                        break;
                }
            }
            return val;
        }

        // 
        static public string LeftLast(string src,string last,bool include)
        {
            if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(last))
                return src;
            int index = src.LastIndexOf(last);
            if(index >= 0)
            {
                src = src.Substring(0, index);
                return include ? src + last : src;
            }
            return src;
        }

        static public string RightLast(string src, string last, bool include)
        {
            if (string.IsNullOrEmpty(src) || string.IsNullOrEmpty(last))
                return src;
            int index = src.LastIndexOf(last);
            if (index >= 0)
            {
                index = include ? index : index + last.Length;
                return src.Substring(index);
            }
            return src;
        }

        static public void DeleteFile(string fn, bool isFilePath)
        {
            string _fp = isFilePath ? fn : curInstance.GetFilePath(fn);
            DelFile(_fp);
        }

        static public void DeleteFile(string fn)
        {
            DeleteFile(fn, false);
        }

        // 取得文本内容
        static public string GetText(string fn)
        {
            string _fp = curInstance.GetPath(fn);
            if (File.Exists(_fp))
            {
                return File.ReadAllText(_fp);
            }

            string _suffix = Path.GetExtension(fn);
            int _ind_ = fn.LastIndexOf(_suffix);
            string _fnNoSuffix = fn.Substring(0, _ind_);
            TextAsset txtAsset = Resources.Load<TextAsset>(_fnNoSuffix); // 可以不用考虑释放txtAsset
            string _ret = "";
            if (txtAsset)
            {
                _ret = txtAsset.text;
                Resources.UnloadAsset(txtAsset);
            }
            return _ret;
        }

        static public string GetDecryptText(string fn)
        {
            string val = GetText(fn);
            return Decrypt(val);
        }

        static public byte[] GetTextBytes(string fn)
        {
            string val = GetText(fn);
            string v64 = Decrypt(val);
            return System.Text.Encoding.UTF8.GetBytes(v64);
        }

        static public void WriteText(string fn, string content, bool isFilePath)
        {
            string _fp = isFilePath ? fn : curInstance.GetFilePath(fn);
            CreateText(_fp, content);
        }

        static public void WriteText(string fn, string content)
        {
            WriteText(fn, content, false);
        }

        // 文件是否存在可读写文件里
        static public bool IsExistsFile(string fn, bool isFilePath)
        {
            string _fp = isFilePath ? fn : curInstance.GetFilePath(fn);
            return File.Exists(_fp);
        }

        // 取得文件流
        static public byte[] GetFileBytes(string fn)
        {
            string _fp = curInstance.GetPath(fn);
            if (File.Exists(_fp))
            {
                return File.ReadAllBytes(_fp);
            }

            string _suffix = Path.GetExtension(fn);
            int _ind_ = fn.LastIndexOf(_suffix);
            string _fnNoSuffix = fn.Substring(0, _ind_);
            TextAsset txtAsset = Resources.Load<TextAsset>(_fnNoSuffix); // 可以不用考虑释放txtAsset
            byte[] _bts = null;
            if (txtAsset)
            {
                _bts = txtAsset.bytes;
                UnLoadOne(txtAsset);
            }
            return _bts;
        }

        static public string m_fpABManifest
        {
            get
            {
                return curInstance.GetPath(m_curPlatform);
            }
        }

        // 对象函数
        virtual public string GetFilePath(string fn)
        {
            return string.Concat(m_dirRes, fn);
        }

        virtual public string GetPath(string fn)
        {
            string _fp = GetFilePath(fn);
            if (IsFile(_fp))
                return _fp;
            return GetStreamingFilePath(fn);
        }
        
        virtual public bool IsLoadOrg4Editor()
        {
            return false;
        }

        virtual public string GetTextDecrypt(string fn)
        {
            return GetDecryptText(fn);
        }
    }
}