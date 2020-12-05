/// <summary>
/// 类名 : Update更新接口
/// 作者 : Canyon
/// 日期 : 2016-06-27 20:37
/// 功能 : 所有Update
/// </summary>
public interface IUpdate {
    bool IsOnUpdate(); // 是否可以调用 OnUpdate 函数
    void OnUpdate(float dt,float unscaledDt);
}
