using UnityEngine;

/// <summary>
/// 类名 : 正6变形布局
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2021-07-22 19:52
/// 功能 : 平铺布局
/// </summary>
public class HexagonLayout : MonoBehaviour
{
	public float edgeSize = 0.9f;// 正六边形边长 = 半径的
	public int column = 10; // 列数
	public int row = 19; // 行数
	
	[ContextMenu("Re-ReView")]
	void ReView()
	{
		float verticalEdgeSize = Mathf.Sin(60 * Mathf.Deg2Rad) * edgeSize;
		Transform _trsf = transform;
		GameObject _gobj;
		while(_trsf.childCount > 1)
		{
			_gobj = _trsf.GetChild(_trsf.childCount - 1).gameObject;
			DestroyImmediate(_gobj);
		}
		
		Transform _first = _trsf.GetChild(0);
		_gobj = _first.gameObject;
		_gobj.SetActive(true);

		_first.name = "1";
		Vector3 _pos = _first.position;
		Vector3 _scl = _first.localScale;
		float _c_scl = _scl.x;

		float _f_x =  _pos.x;
		float _f_z =  _pos.z;
		float _fix_f_x = 1.5f * edgeSize * _c_scl;
		float _fix_f_z = verticalEdgeSize * _c_scl;
		float _fix_z = _fix_f_z * 2;
		
		Transform _trsfCurr;
		Vector3 _posCur = _pos;
		float _c_x =  _f_x;
		float _c_z = _f_z;
		bool _is1 = false;

		for(int i = 0; i < row; i++)
		{
			if(i != 0)
			{
				_is1 = i % 2 == 1;
				if(_is1)
				{
					_f_x += _fix_f_x;
					_f_z -= _fix_f_z;
				}else{
					_f_x += _fix_f_x;
					_f_z += _fix_f_z;
				}
			}
			
			for(int j = 0; j < column; j++)
			{
				if(j == 0)
				{
					_c_x = _f_x;
					_c_z = _f_z;
				}

				if(i == 0 && j == 0)
					continue;
				
				_gobj = GameObject.Instantiate(_first.gameObject,_trsf,false);
				_gobj.name = (i * column + j + 1) + "";
				_trsfCurr = _gobj.transform;
				_trsfCurr.localScale = _scl;
				if(j != 0)
					_c_z -= _fix_z;
				_posCur.x = _c_x;
				_posCur.z = _c_z;
				_trsfCurr.position = _posCur;
			}
		}
		_ReBindPElement();
	}

	void _ReBindPElement()
	{
		PrefabElement _csElement = PrefabElement.Get(gameObject);
		if(_csElement)
		{
			// _csElement.ReNodes();
		}
	}
}
