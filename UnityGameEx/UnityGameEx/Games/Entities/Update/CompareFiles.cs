using System.Collections.Generic;

namespace Core.Kernel
{
    /// <summary>
    /// 类名 : 对比文件
    /// 作者 : Canyon / 龚阳辉
    /// 日期 : 2017-12-07 10:35
    /// 功能 : 
    /// </summary>
    public class CompareFiles
    {
        static int _Code = 0;
        static SortCompareFiles m_sort = new SortCompareFiles();

        public int m_code { get; private set; }
        public string m_newUrl { get; private set; } // 更新地址
        public string m_newProj { get; private set; } // file proj (下载地址是url + proj + filename)
        public string m_newContent { get; private set; }
        
        public CfgFileList m_cfgOld { get; private set; }
        public CfgFileList m_cfgNew { get; private set; }
        protected Dictionary<string, ResInfo> m_deletes { get; private set; }
        public Dictionary<string, ResInfo> m_updates { get; private set; }

        List<ResInfo> m_lDownError = new List<ResInfo>();
        List<ResInfo> m_lDowning = new List<ResInfo>();
        List<string> m_keys = new List<string>();

        public EM_CompFiles m_state { get; private set; }
        public DF_LDownFile m_callDownFile; // 下载文件的状态通知 参数:值(当前文件对象)
        public bool m_isDownAll { get; set; } // 是否下载全部文件
        public int m_maxDown = 5; // 最大下载个数
        public long m_sumDownSize { get; private set; }
        public string m_strError { get; private set; }

        public int m_iState { get { return (int)m_state; } }
        public bool isError { get { return m_iState >= (int)EM_CompFiles.Error; } }
        public bool isFinished { get { return m_state == EM_CompFiles.Completed; } }

        public CompareFiles()
        {
            m_code = (++_Code);
            m_cfgOld = CfgFileList.Builder();
            m_cfgNew = CfgFileList.Builder();
            m_deletes = new Dictionary<string, ResInfo>();
            m_updates = new Dictionary<string, ResInfo>();
            m_state = EM_CompFiles.Init;
        }

        public void ClearAll()
        {
            m_cfgOld.Clear();
            m_cfgNew.Clear();
            Clear();
        }

        void Clear()
        {
            m_deletes.Clear();
            m_updates.Clear();
            m_lDownError.Clear();
            m_lDowning.Clear();
            m_keys.Clear();
        }

        public void Init(string newFiles, string newUrl, string newProj)
        {
            Init(CfgFileList.BuilderDefault(), newFiles, newUrl, newProj);
        }

        public void Init(CfgFileList oldFiles, string newFiles, string newUrl, string newProj)
        {
            Clear();

            this.m_newUrl = newUrl;
            this.m_newProj = newProj;
            this.m_newContent = newFiles;

            this.m_cfgOld.CloneFromOther(oldFiles);
            this.m_cfgNew.Init(newFiles);

            this.DoCompare();
        }

        public void DoCompare()
        {
            if (m_cfgNew.GetDataCount() <= 0)
                return;

            ResInfo _tmp1, _tmp2;
            foreach (string _key in m_cfgOld.m_data.m_dic.Keys)
            {
                if (m_cfgNew.IsHas(_key))
                    continue;
                _tmp1 = m_cfgOld.GetInfo(_key);
                m_deletes.Add(_key, _tmp1);
            }

            foreach (string _key in m_cfgNew.m_data.m_dic.Keys)
            {
                _tmp1 = m_cfgNew.GetInfo(_key);
                if (m_cfgOld.IsHas(_key))
                {
                    _tmp2 = m_cfgOld.GetInfo(_key);
                    if (_tmp1.IsSame(_tmp2))
                        continue;
                }
                _tmp1.DownReady(m_newUrl, m_newProj, _CFLoadDown, EM_Asset.Bytes, 1, 2);
                m_updates.Add(_key, _tmp1);
            }
        }

        // 合并
        public void Merge(CompareFiles other)
        {
            int _diff = other.m_code - this.m_code;
            if (_diff == 0)
                return;

            CompareFiles min = _diff > 0 ? this : other;
            CompareFiles max = _diff > 0 ? other : this;

            foreach (string _key in min.m_deletes.Keys)
            {
                if (max.m_deletes.ContainsKey(_key))
                    continue;

                if (max.m_updates.ContainsKey(_key))
                    continue;

                max.m_deletes.Add(_key, min.m_deletes[_key]);
            }

            foreach (string _key in min.m_updates.Keys)
            {
                if (max.m_updates.ContainsKey(_key))
                    continue;

                if (max.m_deletes.ContainsKey(_key))
                    continue;

                max.m_updates.Add(_key, min.m_updates[_key]);
            }

            foreach (string _key in max.m_updates.Keys)
            {
                if (max.m_deletes.ContainsKey(_key))
                {
                    this.m_keys.Add(_key);
                }
            }

            for (int i = 0; i < this.m_keys.Count; i++)
            {
                max.m_updates.Remove(this.m_keys[i]);
            }

            this.m_keys.Clear();
            min.ClearAll();
        }

        public CompareFiles MergeGetMax(List<CompareFiles> list)
        {
            if (list == null || list.Count <= 0)
                return this;

            if (!list.Contains(this))
            {
                list.Add(this);
            }

            if (list.Count == 1)
                return list[0];

            list.Sort(m_sort);

            CompareFiles min = list[0];
            list.RemoveAt(0);

            CompareFiles max = list[0];
            list.RemoveAt(0);
            while (max.m_code != min.m_code)
            {
                min.Merge(max);
                min = max;

                if (list.Count > 0)
                {
                    max = list[0];
                    list.RemoveAt(0);
                }
            }
            return max;
        }

        public void OnUpdate()
        {
            switch (m_state)
            {
                case EM_CompFiles.Init:
                    m_state = EM_CompFiles.CheckDelFiles;
                    break;
                case EM_CompFiles.CheckDelFiles:
                    _ST_CheckDelFiles();
                    break;
                case EM_CompFiles.DelFiles:
                    _ST_DelFiles();
                    break;
                case EM_CompFiles.CheckDownFiles:
                    _ST_CheckDownFiles();
                    break;
                case EM_CompFiles.DownFiles:
                    _ST_DownFiles();
                    break;
                case EM_CompFiles.Completed:
                    break;
                default:
                    break;
            }
        }

        void _ST_CheckDelFiles()
        {
            bool _isSaveNd = false;
            bool _isState = false;
            foreach (string _kk in this.m_deletes.Keys)
            {
                _isState = CfgFileList.instanceNeedDown.Remove(_kk);
                if (_isState)
                    _isSaveNd = true;
            }

            this.m_keys.AddRange(this.m_updates.Keys);
            string _key = null;
            ResInfo _tmp = null;
            bool _isMust = false;
            for (int i = 0; i < this.m_keys.Count; i++)
            {
                _key = this.m_keys[i];
                _tmp = this.m_updates[_key];
                this.m_deletes.Add(_key, _tmp);

                if (!m_isDownAll)
                {
                    _isMust = _tmp.m_isMustFile || CfgMustFiles.instance.IsMust(_tmp.m_curName);
                    if (!_isMust)
                    {
                        this.m_updates.Remove(_key);
                        _isSaveNd = true;
                        CfgFileList.instanceNeedDown.Add(_tmp);
                        continue;
                    }
                }
                this.m_sumDownSize += _tmp.m_size;
            }

            if (_isSaveNd)
                CfgFileList.instanceNeedDown.SaveByTContent();

            this.m_keys.Clear();
            m_state = EM_CompFiles.DelFiles;
        }

        void _ST_DelFiles()
        {
            if (m_deletes.Count <= 0)
            {
                m_state = EM_CompFiles.CheckDownFiles;
                return;
            }

            this.m_keys.AddRange(this.m_deletes.Keys);
            int lens = this.m_keys.Count;
            ResInfo _tmp;
            string _key;
            for (int i = 0; i < 10; i++)
            {
                if (lens > i)
                {
                    _key = this.m_keys[i];
                    _tmp = m_deletes[_key];
                    m_deletes.Remove(_key);
                    UGameFile.DeleteFile(_tmp.m_resName);
                }
            }
            this.m_keys.Clear();
        }

        void _ST_CheckDownFiles()
        {
            if (m_lDownError.Count <= 0)
            {
                m_state = EM_CompFiles.DownFiles;
                return;
            }

            List<ResInfo> _list = new List<ResInfo>(m_lDownError);
            m_lDownError.Clear();
            ResInfo _tmp;
            for (int i = 0; i < _list.Count; i++)
            {
                _tmp = _list[i];
                if (m_updates.ContainsKey(_tmp.m_curName))
                    continue;
                m_updates.Add(_tmp.m_curName, _tmp);
            }
        }

        void _ST_DownFiles()
        {
            int lens = this.m_updates.Count;
            int lens2 = this.m_lDowning.Count;
            if (lens <= 0 && lens2 <= 0)
            {
                if (m_state == EM_CompFiles.DownFiles)
                    m_state = EM_CompFiles.Completed;
                return;
            }

            if (lens2 >= this.m_maxDown)
                return;

            this.m_keys.AddRange(this.m_updates.Keys);
            ResInfo _tmp;
            string _key;
            for (int i = 0; i < this.m_maxDown - lens2; i++)
            {
                if (lens > i)
                {
                    _key = this.m_keys[0];
                    this.m_keys.RemoveAt(0);
                    _tmp = this.m_updates[_key];
                    this.m_updates.Remove(_key);
                    _tmp.DownStartCheckCode();
                    this.m_lDowning.Add(_tmp);
                }
            }
            this.m_keys.Clear();
        }

        void _CFLoadDown(int state, ResInfo dlFile)
        {
            bool isSuccess = state == (int)EM_SucOrFails.Success;
            if (isSuccess)
            {
                bool _isMust = (dlFile != null) && (dlFile.m_isMustFile || CfgMustFiles.instance.IsMust(dlFile.m_curName));
                CfgFileList.instanceDown.Save2Downed(dlFile, _isMust);
            }
            else
            {
                this.m_state = (EM_CompFiles)dlFile.m_iDownState;
                this.m_strError = dlFile.m_strError;
                this.m_lDownError.Add(dlFile);
            }

            this.m_lDowning.Remove(dlFile);

            if (this.m_callDownFile != null)
                this.m_callDownFile(state, dlFile);
        }
        
        public void ReDownFile()
        {
            if (m_lDownError.Count <= 0)
                return;
            for (int i = 0; i < m_lDownError.Count; i++)
            {
                m_lDownError[i].ReDownReady(_CFLoadDown);
            }
            this.m_state = EM_CompFiles.CheckDownFiles;
        }

        public bool SaveFileList()
        {
            return m_cfgNew.Save2Default();
        }
    }

    class SortCompareFiles : Comparer<CompareFiles>
    {
        #region implemented abstract members of Comparer
        public override int Compare(CompareFiles x, CompareFiles y)
        {
            return x.m_code.CompareTo(y.m_code);
        }
        #endregion

    }
}
