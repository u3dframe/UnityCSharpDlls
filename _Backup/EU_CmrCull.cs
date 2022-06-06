using UnityEngine;

/// <summary>
/// 类名 : 优化1 - 视锥体剔除(Frustum Culling)
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2022-03-08 19:15
/// 功能 : 
/// </summary>
public class EU_CmrCull : MonoBehaviour 
{
    static public EU_CmrCull Get(GameObject gobj,bool isAdd = true)
    {
        EU_CmrCull _ret = null;
        if(gobj){
            _ret = gobj.GetComponent<EU_CmrCull>();
            if(!_ret && isAdd)
                _ret = gobj.AddComponent<EU_CmrCull>();
            _ret?.Init();
        }
        return _ret;
    }

    [SerializeField] Renderer[] m_alls = null;
    bool _isInit = false;
    int _allLens = 0;
    private Plane[] planes;
    [SerializeField] bool _isRunning = false;
    [SerializeField] Camera m_currCmr = null;

    void Start()
    {
        this.Init();
    }

    public void Init(){
        if(_isInit)
            return;
        _isInit = true;
        planes = new Plane[6];
        m_alls = this.gameObject.GetComponentsInChildren<Renderer>(true);
        _allLens = m_alls.Length;

        if(!m_currCmr)
            m_currCmr = Camera.main;
        
        // _isRunning = true;
    }

    void Update()
    {
        if(!_isRunning || !_isInit || m_alls == null || _allLens <= 0)
            return;
        
        var cmr = m_currCmr;
        if(!cmr)
            return;

        this._UseBounds(cmr);
    }

    void _UseBounds(Camera cmr)
	{
        GeometryUtility.CalculateFrustumPlanes(cmr,planes);

        Renderer _cur;
        Bounds _bounds;
        bool _result;
        bool _isChg = false;
        for (int i = 0; i < _allLens; ++i)
        {
            _cur = this.m_alls[i];
            if(!_cur){
                _isChg = true;
                continue;
            }
            _bounds = _cur.bounds;
            _result = GeometryUtility.TestPlanesAABB(planes,_bounds);
            _cur.enabled = _result;
        }

        if(_isChg)
            this._ReSetAlls();
	}

    void _ReSetAlls(){
        Renderer _cur;
        var list = new System.Collections.Generic.List<Renderer>();
        for (int i = 0; i < _allLens; ++i)
        {
            _cur = this.m_alls[i];
            if(!_cur)
                continue;
            list.Add(_cur);
        }
        this.m_alls = list.ToArray();
        _allLens = this.m_alls.Length;
    }
}
