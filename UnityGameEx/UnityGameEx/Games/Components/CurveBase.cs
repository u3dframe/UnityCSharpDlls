using UnityEngine;

public enum CurveExEnum
{
    Default = 0,
    Position = 1,
    LocalPosition = 2,
    LocalAngle = 3,
    LocalScale = 4,
}

/// <summary>
/// 类名 : 曲线 Curve 扩展脚本
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-08-22 22:17
/// 功能 : 
/// </summary>
public class CurveBase : MonoBehaviour
{
    static public CurveBase GetInChild(GameObject gobj)
    {
        if (gobj)
        {
            CurveBase ret = gobj.GetComponent<CurveBase>();
            if (!ret)
            {
                ret = gobj.GetComponentInChildren<CurveBase>();
            }
            return ret;
        }
        return null;
    }

    public CurveExEnum m_emType = CurveExEnum.LocalPosition;
    public int m_indexCurve = -1;
    [SerializeField] AnimationCurve[] m_curves1 = new AnimationCurve[0]; // 对应 x
    [SerializeField] AnimationCurve[] m_curves2 = new AnimationCurve[0]; // 对应 y
    [SerializeField] AnimationCurve[] m_curves3 = new AnimationCurve[0]; // 对应 z

#if UNITY_EDITOR
    bool isRunning = false;
    void Update() {
        if(!isRunning)
            return;
        
        this.ReVal(Time.time,1);
    }

    [ContextMenu("Running")]
    void ED_Running(){
        this.isRunning = true;
    }
#endif

    AnimationCurve RndCurve(AnimationCurve[] arrs,ref int index){
        if(arrs == null || arrs.Length <= 0)
            return null;
        
        int _lens = arrs.Length;
        if(index < 0){
            if(_lens > 1){
                index = Random.Range(0,_lens);
                index %= _lens;
            }else{
                index = 0;
            }
        }

        int _index_ = index;
        if(_index_ >= _lens){
            _index_ = _lens - 1;
        }
        return arrs[_index_];
    }

    public void Evaluate(float time, ref float val1, ref float val2, ref float val3)
    {
        if (time <= 0)
            time = 0;

        if (time >= 1)
            time = 1;

        val1 = 0;
        AnimationCurve _curve_ = RndCurve(this.m_curves1,ref this.m_indexCurve);
        if (_curve_ != null)
        {
            val1 = _curve_.Evaluate(time);
        }

        val2 = 0;
        _curve_ = RndCurve(this.m_curves2,ref this.m_indexCurve);
        if (_curve_ != null)
        {
            val2 = _curve_.Evaluate(time);
        }

        val3 = 0;
        _curve_ = RndCurve(this.m_curves3,ref this.m_indexCurve);
        if (_curve_ != null)
        {
            val3 = _curve_.Evaluate(time);
        }
    }

    public float ReVal(float time)
    {
        float v1 = 0, v2 = 0, v3 = 0;
        Evaluate(time, ref v1, ref v2, ref v3);
        Vector3 _v3;
        switch (this.m_emType)
        {
            case CurveExEnum.Position:
                _v3 = this.transform.position;
                _v3.x += v1;
                _v3.y += v2;
                _v3.z += v3;
                this.transform.position = _v3;
                break;
            case CurveExEnum.LocalPosition:
                this.transform.localPosition = new Vector3(v1, v2, v3);
                break;
            case CurveExEnum.LocalAngle:
                this.transform.localEulerAngles = new Vector3(v1, v2, v3);
                break;
            case CurveExEnum.LocalScale:
                this.transform.localScale = new Vector3(v1, v2, v3);
                break;
            default:
                if (v1 == 0)
                    v1 = v2;
                if (v1 == 0)
                    v1 = v3;
                break;
        }
        return v1;
    }

    public float ReVal(float curtime, float maxtime)
    {
        if (maxtime <= 0)
            return 0;

        float ti = (curtime % maxtime) / maxtime;
        return ReVal(ti);
    }
}
