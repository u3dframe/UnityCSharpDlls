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
		static SortCompareFiles m_sort = new SortCompareFiles ();

		public int m_code{ get; private set; }
		public string m_newUrl{ get; private set; } // 更新地址
        public string m_newProj{ get; private set; } // file proj (下载地址是url + proj + filename)
        public string m_newContent{ get; private set; }

        CfgFileList _m_cfgOld = CfgFileList.Builder();
        public CfgFileList m_cfgOld{ get { return _m_cfgOld; } }
		CfgFileList _m_cfgNew = CfgFileList.Builder();
		public CfgFileList m_cfgNew{ get { return _m_cfgNew; } }

		Dictionary<string,ResInfo> m_deletes = new Dictionary<string, ResInfo> ();
		Dictionary<string, ResInfo> _m_updates = new Dictionary<string, ResInfo> ();
		public Dictionary<string, ResInfo> m_updates{ get { return _m_updates; } }

		int _needDownCount = -1;
		List<ResInfo> m_lDownError = new List<ResInfo> ();

		string m_key = "";
		List<string> m_lKeys = new List<string> ();

		bool m_isRunning = true;
		public EM_Process m_state = EM_Process.None;
		public int m_iState{ get { return (int)m_state; } }
		public bool isError{get{ return m_iState >= (int)EM_Process.Error;} }
		public bool isFinished{ get{ return m_state == EM_Process.Completed; } }
		// 下载文件的状态通知 参数:值(当前文件对象)
		public DF_LDownFile m_callDownFile;

		int _lMin = (int)EM_Process.Error_NotEnoughMemory;
		int _lMax = (int)EM_Process.Error_DownFiles;
		bool _isDoDelUps = false;
		public bool m_isDownAll = false; // 是否下载全部文件

		public CompareFiles ()
		{
			m_code = (++_Code);
		}

		void ClearAll(){
			m_cfgOld.Clear ();
			m_cfgNew.Clear ();

			Clear ();
		}

		void Clear ()
		{
			m_deletes.Clear ();
			m_updates.Clear ();

			m_lKeys.Clear ();
			m_lDownError.Clear ();
		}

		public void Init (string newFiles, string newUrl,string newProj)
		{
			Init (CfgFileList.BuilderDefault(), newFiles, newUrl, newProj);
		}

		public void Init (CfgFileList oldFiles, string newFiles, string newUrl,string newProj)
		{
			Clear ();

			this.m_newUrl = newUrl;
			this.m_newContent = newFiles;
			this.m_newProj = newProj;

			this.m_cfgOld.CloneFromOther (oldFiles);
			this.m_cfgNew.Init (newFiles);

			this.DoCompare ();
		}

		public void DoCompare(){
			this._Compare ();
			m_state = EM_Process.Init;
		}

		void _Compare ()
		{
			if (m_cfgNew.GetDataCount() <= 0)
				return;

            ResInfo _tmp1, _tmp2;
            foreach (string _key in m_cfgOld.m_data.m_dic.Keys) {
				if (m_cfgNew.IsHas (_key))
					continue;
                _tmp1 = m_cfgOld.GetInfo(_key);
                m_deletes.Add (_key, _tmp1);
			}

			foreach (string _key in m_cfgNew.m_data.m_dic.Keys) {
                _tmp1 = m_cfgNew.GetInfo(_key);
				if (m_cfgOld.IsHas (_key)) {
                    _tmp2 = m_cfgOld.GetInfo(_key);
                    if (_tmp1.IsSame(_tmp2))
						continue;
				}
                _tmp1.DownReady(m_newUrl, m_newProj, _CFLoadDown,EM_Asset.Bytes,1,2);
				m_updates.Add (_key, _tmp1);
			}
		}

		// 合并
		public void Merge (CompareFiles other)
		{
			int _diff = other.m_code - this.m_code;
			if (_diff == 0)
				return;

			CompareFiles min = _diff > 0 ? this : other;
			CompareFiles max = _diff > 0 ? other : this;

			foreach (string _key in min.m_deletes.Keys) {
				if (max.m_deletes.ContainsKey (_key))
					continue;
				
				if (max.m_updates.ContainsKey (_key))
					continue;
				
				max.m_deletes.Add (_key, min.m_deletes [_key]);
			}

			foreach (string _key in min.m_updates.Keys) {
				if (max.m_updates.ContainsKey (_key))
					continue;

				if (max.m_deletes.ContainsKey (_key))
					continue;

				max.m_updates.Add (_key, min.m_updates [_key]);
			}

			m_lKeys.Clear ();

			foreach (string _key in max.m_updates.Keys) {
				if (max.m_deletes.ContainsKey (_key)) {
					m_lKeys.Add (_key);
				}
			}

			for (int i = 0; i < m_lKeys.Count; i++) {
				max.m_updates.Remove (m_lKeys [i]);
			}

			m_lKeys.Clear ();
			min.ClearAll ();
		}

		public CompareFiles MergeGetMax (List<CompareFiles> list)
		{
			if (list == null || list.Count <= 0)
				return this;
			
			if (!list.Contains (this)) {
				list.Add (this);
			}

			if (list.Count == 1)
				return list [0];

			list.Sort (m_sort);

			CompareFiles min = list [0];
			list.RemoveAt (0);

			CompareFiles max = list [0];
			list.RemoveAt (0);
			while (max.m_code != min.m_code) {
				min.Merge (max);
				min = max;

				if (list.Count > 0) {
					max = list [0];
					list.RemoveAt (0);
				}
			}
			return max;
		}

		public void OnUpdate ()
		{
			if (!m_isRunning)
				return;
			
			switch (m_state) {
			case EM_Process.Init:
				m_state = EM_Process.DeleteFiles;
				break;
			case EM_Process.DeleteFiles:
				_ST_DeleteFiles ();
				break;
			case EM_Process.PreDownFiles:
				_ST_PreDownFiles ();
				break;
			case EM_Process.DownFiles:
				_ST_DownFiles ();
				break;
			case EM_Process.Completed:
				m_isRunning = false;
				break;
			default:
				break;
			}
		}

		void _ST_DeleteFiles ()
		{
			if (m_deletes.Count <= 0) {
				m_state = EM_Process.PreDownFiles;
				return;
			}

			m_lKeys.Clear ();
			m_lKeys.AddRange (m_deletes.Keys);

			int lens = m_lKeys.Count;
			ResInfo info;
			for (int i = 0; i < 10; i++) {
				if (lens > i) {
					m_key = m_lKeys [i];
					info = m_deletes [m_key];
					m_deletes.Remove (m_key);
					UGameFile.DeleteFile (info.m_resName);
				}
			}
		}

		void _JugdeDelFile4End (){
			// 删除需要下载的资源
			if (!_isDoDelUps) {
				_isDoDelUps = true;
				bool _isSaveNd = false;
				foreach (string _key in this.m_deletes.Keys) {
					CfgFileList.instanceNeedDown.Remove (_key);
					_isSaveNd = true;
				}

				ResInfo _tmp = null;
				List<string> _rmKeys = new List<string> ();
				bool _isMust = false;
				foreach (string _key in m_updates.Keys) {
					_tmp = m_updates [_key];
					this.m_deletes.Add (_key, _tmp);

					if (!m_isDownAll) {
						_isMust = _tmp.m_isMustFile || CfgMustFiles.instance.IsMust (_tmp.m_curName);
						if (!_isMust) {
							_rmKeys.Add (_key);
						}
					}
				}

				if (_rmKeys.Count > 0) {
					string _delKey = "";
					_isSaveNd = true;
					for (int i = 0; i < _rmKeys.Count; i++) {
						_delKey = _rmKeys [i];
						_tmp = m_updates [_delKey];
						m_updates.Remove (_delKey);
						CfgFileList.instanceNeedDown.Add (_tmp);
					}
				}
				if (_isSaveNd) {
					CfgFileList.instanceNeedDown.SaveByTContent ();
				}
			}
		}

		void _ST_PreDownFiles(){
			if(_needDownCount <= 0)
				_needDownCount = m_updates.Count;

			if (m_lDownError.Count <= 0) {
				m_state = EM_Process.DownFiles;
				return;
			}

            ResInfo _tmp;
            _needDownCount += m_lDownError.Count;
			for (int i = 0; i < m_lDownError.Count; i++) {
                _tmp = m_lDownError [i];
				if (m_updates.ContainsKey (_tmp.m_resName))
					continue;
				m_updates.Add (_tmp.m_resName, _tmp);
			}

			m_lDownError.Clear ();
		}

		void _ST_DownFiles ()
		{
			if (_needDownCount <= 0) {
				if (m_state == EM_Process.DownFiles)
					m_state = EM_Process.Completed;
				return;
			}

			if (m_updates.Count > 0) {
				m_lKeys.Clear ();
				m_lKeys.AddRange (m_updates.Keys);

                ResInfo _tmp;
                while (m_lKeys.Count > 0) {
					m_key = m_lKeys [0];
					m_lKeys.RemoveAt (0);
                    _tmp = m_updates [m_key];
					m_updates.Remove (m_key);
                    _tmp.DownStart();
				}
				m_lKeys.Clear ();
			}
		}

		void _CFLoadDown(int state,ResInfo dlFile){
			bool isSuccess = state == (int)EM_SucOrFails.Success;
			bool isError = state == (int)EM_SucOrFails.Fails;
			if (isSuccess) {
				CfgFileList.instanceDown.Save2Downed (dlFile);
			}
			if (isSuccess || isError) {
				if (isError) {
					int _lCurr = dlFile.m_iDownState;
					if (_lCurr > _lMin && _lCurr < _lMax) {
						m_state = EM_Process.Error_DownFiles;
					} else {
						m_state = (EM_Process)_lCurr;
					}
					m_lDownError.Add (dlFile);
				}

				_needDownCount--;
			}

            if(this.m_callDownFile != null)
            {
                this.m_callDownFile(state, dlFile);
            }
		}

		// 在比较完备后，会调用取得当前文件的下载大小
		public long GetDownSize(){
			_JugdeDelFile4End ();
			long sum = 0;
			foreach (var item in m_updates.Values) {
				sum += item.m_size;
			}
			return sum;
		}

		public void ReDownFile(){
			if (m_lDownError.Count > 0) {
				for (int i = 0; i < m_lDownError.Count; i++) {
                    m_lDownError[i].ReDownReady(_CFLoadDown);
				}
			}

			this.m_state = EM_Process.PreDownFiles;
			m_isRunning = true;
		}

		public bool Save ()
		{
			return m_cfgNew.Save2Default ();
		}
	}

	class SortCompareFiles : Comparer<CompareFiles>
	{
		#region implemented abstract members of Comparer
		public override int Compare (CompareFiles x, CompareFiles y)
		{
			return x.m_code.CompareTo (y.m_code);
		}
		#endregion
		
	}
}
