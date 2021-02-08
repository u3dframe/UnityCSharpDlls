using UnityEngine;

public enum LookType
{
    Trsf,
    Vec3
}

/// <summary>
/// 类名 : 平滑朝向者
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2015-05-11 09:29
/// 功能 : 
/// </summary>
public class SmoothLookAt : MonoBehaviour
{
    static public SmoothLookAt Get(Object uobj, bool isAdd)
    {
        return GHelper.Get<SmoothLookAt>(uobj, isAdd);
    }

    static public SmoothLookAt Get(Object uobj)
    {
        return Get(uobj, true);
    }

    public LookType m_lookType = LookType.Trsf;
    public Transform target;
    public bool isUpByLate = false;
    public bool isRunningAt = false;
    public bool m_smoothAt = false;
    [HideInInspector] public float dampingAt = 2.2f; // 6
    [HideInInspector] public Vector3 v3Offset = Vector3.zero;
    [HideInInspector] public Vector3 v3Target = Vector3.zero;
    private Transform _trsf = null;
    protected Transform m_trsf { get { if (_trsf == null) _trsf = transform; return _trsf; } }

    void Start()
    {
        Rigidbody _rbody = GetComponent<Rigidbody>();
        if (_rbody)
            _rbody.freezeRotation = true;
    }

    void Update()
    {
        if (isUpByLate) return;
        _OnUpdate();
    }

    void LateUpdate()
    {
        if (!isUpByLate) return;
        _OnUpdate();
    }

    virtual protected void _OnUpdate()
    {
        if (!isRunningAt)
            return;

        switch (m_lookType)
        {
            case LookType.Trsf:
                _LookTrsf();
                break;
            case LookType.Vec3:
                _LookVec3(v3Target);
                break;
        }
    }

    void _LookTrsf()
    {
        if (!target) return;
        Vector3 _pos = target.position + v3Offset;
        this._LookVec3(_pos);
    }

    void _LookVec3(Vector3 dest)
    {
        if (m_smoothAt)
        {
            var rotation = Quaternion.LookRotation(dest - m_trsf.position);
            m_trsf.rotation = Quaternion.Slerp(m_trsf.rotation, rotation, Time.deltaTime * dampingAt);
            return;
        }
        m_trsf.LookAt(dest);
    }

    public void LookAt(Object target,float ox,float oy,float oz,bool smoothAt = true, bool upLate = false)
    {
        Transform _t = GHelper.ToTransform(target);
        this.target = _t;
        this.v3Offset.x = ox;
        this.v3Offset.y = oy;
        this.v3Offset.z = oz;
        this.m_smoothAt = smoothAt;
        this.isUpByLate = upLate;
        this.m_lookType = LookType.Trsf;
        this.isRunningAt = true;
    }

    public void LookAt(float tx, float ty, float tz, bool smoothAt = true, bool upLate = false)
    {
        this.v3Target.x = tx;
        this.v3Target.y = ty;
        this.v3Target.z = tx;
        this.m_smoothAt = smoothAt;
        this.isUpByLate = upLate;
        this.m_lookType = LookType.Vec3;
        this.isRunningAt = true;
    }
}