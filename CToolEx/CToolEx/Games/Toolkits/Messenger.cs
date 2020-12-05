using System;
using System.Collections.Generic;

namespace Core.Kernel{
	/// <summary>
	/// 类名 : 信使 - 送信人
	/// 作者 : Canyon / 龚阳辉
	/// 日期 : 2017-02-07 09:33
	/// 功能 : 消息机制
	/// </summary>	
	static public class Messenger {
		
		private static Dictionary<string,Delegate> eventTable = new Dictionary<string, Delegate>();

		static public void AddListener(string eventType,Action handler){
			lock (eventTable) {
				if (!eventTable.ContainsKey (eventType)) {
					eventTable.Add (eventType, null);
				}

				eventTable [eventType] = (Action)eventTable [eventType] + handler;
			}
		}

		static public void RemoveListener(string eventType,Action handler){
			lock (eventTable) {
				if (eventTable.ContainsKey (eventType)) {
					eventTable [eventType] = (Action)eventTable [eventType] - handler;

					if (eventTable [eventType] == null) {
						eventTable.Remove (eventType);
					}
				}
			}
		}

		static public void Brocast(string eventType){
			Delegate d;
			if (eventTable.TryGetValue (eventType, out d)) {
				Action callback = (Action)d;

				if (callback != null) {
					callback ();
				}
			}
		}


		static public void AddListener<T>(string eventType,Action<T> handler){
			lock (eventTable) {
				if (!eventTable.ContainsKey (eventType)) {
					eventTable.Add (eventType, null);
				}

				eventTable [eventType] = (Action<T>)eventTable [eventType] + handler;
			}
		}

		static public void RemoveListener<T>(string eventType,Action<T> handler){
			lock (eventTable) {
				if (eventTable.ContainsKey (eventType)) {
					eventTable [eventType] = (Action<T>)eventTable [eventType] - handler;

					if (eventTable [eventType] == null) {
						eventTable.Remove (eventType);
					}
				}
			}
		}

		static public void Brocast<T>(string eventType,T arg1){
			Delegate d;
			if (eventTable.TryGetValue (eventType, out d)) {
				Action<T> callback = (Action<T>)d;

				if (callback != null) {
					callback (arg1);
				}
			}
		}

		static public void AddListener<T1,T2>(string eventType,Action<T1,T2> handler){
			lock (eventTable) {
				if (!eventTable.ContainsKey (eventType)) {
					eventTable.Add (eventType, null);
				}

				eventTable [eventType] = (Action<T1,T2>)eventTable [eventType] + handler;
			}
		}

		static public void RemoveListener<T1,T2>(string eventType,Action<T1,T2> handler){
			lock (eventTable) {
				if (eventTable.ContainsKey (eventType)) {
					eventTable [eventType] = (Action<T1,T2>)eventTable [eventType] - handler;

					if (eventTable [eventType] == null) {
						eventTable.Remove (eventType);
					}
				}
			}
		}

		static public void Brocast<T1,T2>(string eventType,T1 arg1,T2 arg2){
			Delegate d;
			if (eventTable.TryGetValue (eventType, out d)) {
				Action<T1,T2> callback = (Action<T1,T2>)d;

				if (callback != null) {
					callback (arg1,arg2);
				}
			}
		}

		static public void AddListener<T1,T2,T3>(string eventType,Action<T1,T2,T3> handler){
			lock (eventTable) {
				if (!eventTable.ContainsKey (eventType)) {
					eventTable.Add (eventType, null);
				}

				eventTable [eventType] = (Action<T1,T2,T3>)eventTable [eventType] + handler;
			}
		}

		static public void RemoveListener<T1,T2,T3>(string eventType,Action<T1,T2,T3> handler){
			lock (eventTable) {
				if (eventTable.ContainsKey (eventType)) {
					eventTable [eventType] = (Action<T1,T2,T3>)eventTable [eventType] - handler;

					if (eventTable [eventType] == null) {
						eventTable.Remove (eventType);
					}
				}
			}
		}

		static public void Brocast<T1,T2,T3>(string eventType,T1 arg1,T2 arg2,T3 arg3){
			Delegate d;
			if (eventTable.TryGetValue (eventType, out d)) {
				Action<T1,T2,T3> callback = (Action<T1,T2,T3>)d;

				if (callback != null) {
					callback (arg1,arg2,arg3);
				}
			}
		}
	}
}
