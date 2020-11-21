using UnityEngine;
using System.IO;

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

        public void DeleteFile(string fn, bool isFilePath)
        {
            string _fp = isFilePath ? fn : GetFilePath(fn);
            DelFile(_fp);
        }

        public void DeleteFile(string fn)
        {
            DeleteFile(fn, false);
        }

        // 取得文本内容
        public string GetText(string fn)
        {
            string _fp = GetPath(fn);
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

        public void WriteText(string fn, string content, bool isFilePath)
        {
            string _fp = isFilePath ? fn : GetFilePath(fn);
            CreateText(_fp, content);
        }

        public void WriteText(string fn, string content)
        {
            WriteText(fn, content, false);
        }

        // 文件是否存在可读写文件里
        public bool IsExistsFile(string fn, bool isFilePath)
        {
            string _fp = isFilePath ? fn : GetFilePath(fn);
            return File.Exists(_fp);
        }

        // 取得文件流
        public byte[] GetFileBytes(string fn)
        {
            string _fp = GetPath(fn);
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

        public string m_fpABManifest
        {
            get
            {
                return GetPath(m_curPlatform);
            }
        }

        virtual public bool IsLoadOrg4Editor()
        {
            return false;
        }
    }
}