using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Core.Kernel{
	/// <summary>
	/// 类名 : 资源加载，下载脚本
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2018-10-18 10:35
	/// 功能 : 
	/// 描述 : 
	/// </summary>
	public class MgrDownload : GobjLifeListener
    {
		static MgrDownload _shareInstance;
		static public MgrDownload shareInstance {
			get {
                if (IsNull(_shareInstance))
                {
                    GameObject _gobj = GameMgr.mgrGobj;
                    _shareInstance = UtilityHelper.Get<MgrDownload>(_gobj, true);
                    _shareInstance.csAlias = "DownMgr";
                }
                return _shareInstance;
			}
		}

        /// <summary>
		/// 0:Net Not,1:Net Phone,2:Net Wifi
		/// </summary>
		public int m_iNetState { get; private set; }
        public int m_iPreNetState { get; private set; }
        public bool m_isNet { get { return this.m_iNetState > 0; } }
        public bool m_isNoNet { get { return !this.m_isNet; } }
        
        public bool isRunning{ get; set;}
		protected bool m_isInit = false;

		// 限定更新频率
		float m_currTime = 0;
        public float m_limitUpTime = 0.03f;
        
        WaitForSecondsRealtime objCustomWait = new WaitForSecondsRealtime(0.1f);
        float _m_waitSec = 0.1f;
		public float m_waitSec{
			get{ return _m_waitSec;} 
			set{
				if (value != _m_waitSec) {
					if (value > 0.01f)
						objCustomWait = new WaitForSecondsRealtime (value);
				}
				_m_waitSec = value;
			}
		}
		
        List<ResInfo> m_needLoads = new List<ResInfo>();  // 需要列表
        Queue<ResInfo> m_preLoads = new Queue<ResInfo>(); // 预先队列

        // 限定下载加载个数
        int _m_nCurr = 0;
        public int m_nLimit = 5;

        // Update is called once per frame
        void Update () {
			_ST_Check_Net_State ();

			if (!isRunning)
				return;
			
			if (m_limitUpTime > 0.02f) {
				m_currTime += Time.deltaTime;
				if (m_currTime < m_limitUpTime) {
					return;
				}
				m_currTime -= m_limitUpTime;
			}

            do
            {
                _ST_LoadDown();
            } while (_isWhile());

        }

        bool _isWhile()
        {
            int _size_pre = this.m_preLoads.Count;
            int _size_nd = this.m_needLoads.Count;
            if (_size_pre <= 0 && _size_nd <= 0)
                return false;
            return this._m_nCurr < this.m_nLimit;
        }
        
        public void Init(){
			if (m_isInit)
				return;
			m_isInit = true;
			m_iNetState = (int)Application.internetReachability;
			m_iPreNetState = m_iNetState;
		}

		void _ST_Check_Net_State(){
			// 0:Net Not,1:Net Phone,2:Net Wifi
			m_iPreNetState = m_iNetState;
			m_iNetState = (int)Application.internetReachability;
		}

		void _ST_LoadDown(){
            if (this._m_nCurr >= this.m_nLimit)
                return;

            int _size_pre = this.m_preLoads.Count;
            int _size_nd = this.m_needLoads.Count;
            if (_size_pre <= 0 && _size_nd <= 0)
            {
                return;
            }

            this._m_nCurr++;

            ResInfo _info = null;
            if (_size_pre > 0)
            {
                _info = this.m_preLoads.Dequeue();
            }
            else
            {
                _info = this.m_needLoads[0];
                this.m_needLoads.Remove(_info);
            }

            if (string.IsNullOrEmpty(_info.m_url))
            {
                var _cfl = CfgVersion.instance;
                _info.DownReady(_cfl.m_urlFilelist, _cfl.m_pkgFiles, _CFLoadDown, EM_Asset.Bytes, 1, 2);
            }
            else
            {
                _info.ReDownReady(_CFLoadDown);
            }
            StartCoroutine(_IEntLoadDown(_info));
		}

        IEnumerator _IEntLoadDown(ResInfo dlFile)
        {
            if (m_waitSec > 0.01f)
                yield return objCustomWait;
            yield return dlFile;
            // yield return null;
            this._m_nCurr--;
            this.isRunning = this.m_needLoads.Count > 0 || this.m_preLoads.Count > 0;
        }

        void _CFLoadDown(int state, ResInfo dlFile)
        {
            bool isSuccess = state == (int)EM_SucOrFails.Success;
            _CFLDownState(isSuccess,dlFile);
            if (!isSuccess)
            {
                AddNeedDown(dlFile,true);
            }
            // Debug.LogErrorFormat ("== _CFLoadDown = [{0}] = [{1}]",dlFile.m_resName,(_call != null));
        }

        public void AddNeedDown(ResInfo dlFile,bool isRunning){
			if (dlFile == null || this.m_preLoads.Contains(dlFile) || this.m_needLoads.Contains(dlFile))
				return;
            this.m_needLoads.Add(dlFile);
            this.isRunning = isRunning;
        }

        public void AddNeedDown(ResInfo dlFile) {
            bool isRun = this.isRunning;
            this.AddNeedDown(dlFile, false);
            this.isRunning = isRun;
        }

		public void AddNeedDowns(params ResInfo[] dlFiles){
			if (dlFiles == null || dlFiles.Length <= 0)
				return;
			int lens = dlFiles.Length;
            bool isRun = this.isRunning;
            for (int i = 0; i < lens; i++) {
                AddNeedDown(dlFiles [i],false);
			}
            this.isRunning = isRun;
		}
        
        public bool AddPreLoad(ResInfo ret,bool isRunning,DF_LDownFile callBack)
        {
            if (ret == null || this.m_preLoads.Contains(ret))
                return false;

            if (this.m_needLoads.Contains(ret))
            {
                this.m_needLoads.Remove(ret);
            }
            ret.AddOnlyOnceCall(callBack);
            this.m_preLoads.Enqueue(ret);
            this.isRunning = isRunning;
            return true;
        }

        public bool AddPreLoad(ResInfo dlFile,DF_LDownFile callBack)
        {
            bool isRun = this.isRunning;
            bool _ret = this.AddPreLoad(dlFile, false, callBack);
            this.isRunning = isRun;
            return _ret;
        }

        public bool AddPreLoad(string resName,bool isRunning,DF_LDownFile callBack)
        {
            ResInfo ret = CfgFileList.instanceDown.GetInfo(resName);
            if (ret == null)
            {
                ret = CfgFileList.instance.GetInfo(resName);
            }
            return AddPreLoad(ret, isRunning, callBack);
        }

        public bool AddPreLoad(string resName,DF_LDownFile callBack)
        {
            bool isRun = this.isRunning;
            bool _ret = this.AddPreLoad(resName, false, callBack);
            this.isRunning = isRun;
            return _ret;
        }

        override protected void OnClear()
        {
            base.OnClear();

            this.m_callDowning = null;
            this.m_callComplate = null;
            this.m_needLoads.Clear();
            this.m_preLoads.Clear();
        }

        // 包体内下载
        bool isCanCallDown = false;
        public int m_iDowned { get; private set; } // 当前下载的个数
        public int m_iAllLens { get; private set; } // 需要下载的个数
        public int m_nSumDownSize { get; private set; } // 下载总大小[B]
        private DF_CurrMax m_callDowning = null;
        private System.Action m_callComplate = null;

        void _CFLDownState(bool isSuccess, ResInfo dlFile)
        {
            if (!this.isCanCallDown)
                return;

            if (isSuccess)
            {
                var cflDown = CfgFileList.instanceDown;
                cflDown.Save2Downed(dlFile);

                this.m_iDowned = this.m_iAllLens - cflDown.GetDataCount();
                this.m_nSumDownSize += dlFile.m_size;
            }

            _ExcCallUpDowning();

            if(this.m_iAllLens <= this.m_iDowned)
            {
                this.m_callDowning = null;
                this._ExcCallCompleted();
            }
        }

        void _ExcCallUpDowning()
        {
            if (this.m_callDowning != null)
                this.m_callDowning(this.m_iDowned,this.m_iAllLens);
        }

        void _ExcCallCompleted()
        {
            this.isCanCallDown = false;
            System.Action _call = this.m_callComplate;
            this.m_callComplate = null;
            if (_call != null)
                _call();
        }

        public MgrDownload InitDown(DF_CurrMax cfDowning,System.Action cfComplate,bool isInitDown)
        {
            this.m_callDowning = cfDowning;
            this.m_callComplate = cfComplate;

            if (isInitDown)
            {
                this.isCanCallDown = true;
                this.m_iDowned = 0;
                this.m_nSumDownSize = 0;

                var cflDown = CfgFileList.instanceDown;
                List<ResInfo> lists = cflDown.GetList(false);
                this.m_iAllLens = lists.Count;
                if (this.m_iAllLens <= 0)
                {
                    this._ExcCallCompleted();
                }
                else
                {
                    for (int i = 0; i < this.m_iAllLens; i++)
                    {
                        this.AddNeedDown(lists[i]);
                    }
                }
            }            
            return this;
        }
    }
}