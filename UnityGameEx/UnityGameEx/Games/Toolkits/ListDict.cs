using System.Collections.Generic;

/// <summary>
/// 类名 : list dic 对象
/// 作者 : Canyon / 龚阳辉
/// 日期 : 2020-06-26 09:33
/// 功能 : 大部分字典操作需要list
/// </summary>
public class ListDict<T> {
    public Dictionary<string, T> m_dic = null; // 字典
    public List<T> m_list = null; // 列表

    public ListDict(bool isList){
        m_dic = new Dictionary<string, T>();
        if(isList){
            m_list = new List<T>();
        }
    }

    public T Get(string key){
        if(m_dic.ContainsKey(key)){
            return m_dic[key];
        }
        return default(T);
    }

    public bool Remove(string key){
        T it = Remove4Get(key);
        return it != null;
    }

    public T Remove4Get(string key){
        if(m_dic.ContainsKey(key)){
            T it = m_dic[key];
            m_dic.Remove(key);

            if(m_list != null)
                m_list.Remove(it);
            
             return it;
        }
        return default(T);
    }

    public bool Add(string key,T it){
        if(m_dic.ContainsKey(key) || it == null){
            return false;
        }
        m_dic.Add(key,it);

        if(m_list != null)
            m_list.Add(it);
        
        return true;
    }

    public void Clear(){
        m_dic.Clear();

         if(m_list != null)
            m_list.Clear();
    }
}
