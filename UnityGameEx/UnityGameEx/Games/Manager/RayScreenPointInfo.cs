using UnityEngine;

/// <summary>
/// 类名 : 屏幕坐标射线对象
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-07-13 21:29
/// 功能 : m_cursor = 游标
/// </summary>
public class RayScreenPointInfo{
	public Vector2 m_pos = Vector2.zero;
	public LayerMask m_layMask = 0;
	public DF_InpRayHit m_call;
	public float m_rayDisctance = Mathf.Infinity;
	public bool m_isAllHit = false;
	
	void _ExcRayHit(Transform hitTrsf){
		if(m_call != null){
			m_call(hitTrsf);
		}
		m_call = null;
	}

	void _ExcRaycastScreenPoint(){
		Camera _main = Camera.main;
		if(!_main){
			_ExcRayHit(null);
			Clear();
			return;
		}

		Ray _ray = _main.ScreenPointToRay(m_pos);
		RaycastHit _hit;
		RaycastHit[] _hits;

		if (m_isAllHit)
		{
			_hits = Physics.RaycastAll(_ray, m_rayDisctance, m_layMask);
			if (_hits != null && _hits.Length > 0)
			{
				int _nlen = _hits.Length;
				for (int i = 0; i < _nlen; i++)
				{
					_ExcRayHit(_hits[i].transform);
				}
			}
			else
			{
				_ExcRayHit(null);
			}
		}
		else
		{
			if (Physics.Raycast(_ray, out _hit, m_rayDisctance, m_layMask))
			{
				// 返回第一个被碰撞到的对象
				_ExcRayHit(_hit.transform);
			}
			else
			{
				_ExcRayHit(null);
			}
		}

		Clear();
	}

	public RayScreenPointInfo DoCast(){
		_ExcRaycastScreenPoint();
		return this;
	}

	public void Clear(){
		m_pos.x = 0;
		m_pos.y = 0;
		m_layMask = 0;
		m_rayDisctance = Mathf.Infinity;
		m_isAllHit = false;
		m_call = null;
	}
}