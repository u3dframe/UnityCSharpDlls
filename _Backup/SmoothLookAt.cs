using UnityEngine;
using UnityEditor;

public enum LookType
{
    T_Trsf,
    T_TrsfV3,
    T_Vec3
}

public class SmoothLookAt : MonoBehaviour
{
    public LookType m_lookType = LookType.T_Trsf;
    public Transform target;
    public bool smooth = true;
    [HideInInspector] public float damping = 2.2f; // 6
    [HideInInspector] public Vector3 v3Target = Vector3.zero;

    void Start()
    {
        Rigidbody _rbody = GetComponent<Rigidbody>();
        if (_rbody)
            _rbody.freezeRotation = true;
    }

    void LateUpdate()
    {
        switch (m_lookType)
        {
            case LookType.T_Trsf:
                _LookTrsf();
                break;
            case LookType.T_TrsfV3:
                if (target) _LookV3(target.position);
                break;
            case LookType.T_Vec3:
                _LookV3(v3Target);
                break;
        }
    }

    void _LookTrsf()
    {
        if (!target) return;
        if (smooth)
        {
            // Look at and dampen the rotation
            var rotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
        }
        else
        {
            // Just lookat
            transform.LookAt(target);
        }
    }

    void _LookV3(Vector3 dest)
    {
        if (smooth)
        {
            // Look at and dampen the rotation
            var rotation = Quaternion.LookRotation(dest - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
        }
        else
        {
            // Just lookat
            transform.LookAt(dest);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SmoothLookAt))]
public class SmoothLookAtInspector : Editor{
	SmoothLookAt m_obj;

	void OnEnable()
    {
        m_obj = target as SmoothLookAt;
    }

	public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

		if(m_obj.smooth){
			m_obj.damping = EditorGUILayout.FloatField("Damping", m_obj.damping);
		}

        if(m_obj.m_lookType == LookType.T_Vec3){
			m_obj.v3Target = EditorGUILayout.Vector3Field("V3Target", m_obj.v3Target);
		}
    }
}
#endif