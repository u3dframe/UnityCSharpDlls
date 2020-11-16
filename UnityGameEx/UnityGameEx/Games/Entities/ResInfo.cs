using UnityEngine;

namespace Core.Kernel
{
	/// <summary>
	/// 类名 : 资源信息对象
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-12-07 14:35
	/// 功能 : 
	/// </summary>
	public class ResInfo : CustomYieldInstruction {
        // 资源名 = m_compareCode
        public string m_resName = "";

        // 真实资源名 - 用于打包时候的判断 (一般是相对路径 xxx/xx/xx.ab,xx.lua,)
        public string m_realName = "";

		// 对比码
		public string m_compareCode = "";

		// 资源的包体(上面的相对路径还在该resPackage下面)
		string _m_resPackage = "";
		public string m_resPackage{
			get{ return _m_resPackage; }
			set{
				_m_resPackage = value;
				if (!string.IsNullOrEmpty (_m_resPackage)) {
					int _ind = _m_resPackage.LastIndexOf ("/");
					if (_ind == _m_resPackage.Length - 1) {
						_m_resPackage = _m_resPackage.Substring (0, _ind);
					}
				}
			}
		}

		// 文件大小
		public int m_size = 0;

		// 文件位置(下载的时候用)
		public string m_filePath {
			get { 
				if (string.IsNullOrEmpty (this.m_resPackage)) {
					return this.m_resName;
				} else {
					return string.Format ("{0}/{1}", this.m_resPackage, this.m_resName);
				}
			}
		}

        public string curName { get { if (string.IsNullOrEmpty(this.m_realName)) return this.m_resName; return this.m_realName; } }
        public bool isManifest { get; private set; }
        public bool m_isMustFile { get; private set; } // 是否是包体必要文件

        public ResInfo(){
		}

		public ResInfo(string row){
			Init (row);
		}

		public ResInfo(string resRealName, string compareCode,string resPackage,int size){
			Init (compareCode, compareCode,resPackage,size,resRealName);
		}

		public void Init(string row){
            string[] _arrs = UGameFile.SplitComma(row);
            int _tmp = _arrs.Length;
            if (_tmp < 3)
				return;
			string resPackage = "";
			if (_tmp > 3)
				resPackage = _arrs [3];

			string resReal = "";
			if (_tmp > 4)
				resReal = _arrs [4];

            _tmp = UtilityHelper.Str2Int(_arrs[2]);
            Init (_arrs [0], _arrs [1],resPackage, _tmp, resReal);
		}

		public void Init(string resName,string compareCode,string resPackage,int size,string realName){
			this.m_resName = resName;
			this.m_compareCode = compareCode;
			this.m_resPackage = resPackage == null ? "" : resPackage;
			this.m_size = size;
			this.m_realName = realName;

            this.isManifest = this.curName.Equals(UGameFile.m_curPlatform);
            this.m_isMustFile = this.isManifest || curName.EndsWith(".lua") || curName.EndsWith(".txt");
        }
        
		public bool IsSame(ResInfo other){
			if (other == null)
				return false;
			int v = string.Compare(other.m_compareCode,m_compareCode,true);
			return v == 0;
		}

		public virtual void CloneFromOther(ResInfo other){
			this.m_resName = other.m_resName;
			this.m_compareCode = other.m_compareCode;
			this.m_resPackage = other.m_resPackage;
			this.m_size = other.m_size;
			this.m_realName = other.m_realName;
		}

		public override bool keepWaiting {
			get {
				return false;
			}
		}

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4}", m_resName, m_compareCode, m_size, m_resPackage, m_realName);
        }
    }
}
