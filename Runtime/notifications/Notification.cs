using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BeatThat.App
{
	/// <summary>
	/// Default Notification object type passed that to listeners by the NotificationBus (listeners can also bind a custom type via generics).
	/// Notifications are identified by their 'type'
	/// Additionally, notifications may pass a single param 'body'
	/// or if a notification has muliple params it may pass key value properties.
	/// </summary>
	public struct Notification 
	{
		public Notification(string type, NotificationReceiverOptions opts = NotificationReceiverOptions.RequireReceiver) : this(type, null, opts)
		{
		}


		public Notification(string type, object body, NotificationReceiverOptions opts = NotificationReceiverOptions.RequireReceiver) : this(type, null, body, opts) 
		{
		}
			
		public Notification(string type, string group, object body, NotificationReceiverOptions opts = NotificationReceiverOptions.RequireReceiver)
		{
			this.type = type;
			this.group = group;
			this.body = body;
			this.receiverOptions = opts;
			m_properties = null;
		}
	
		/// <summary>
		/// Main id for a notification. 
		/// Notification handlers generally register their interests by Notification type.
		/// </summary>
		public string type 
		{
			get; private set;
		}

		/// <summary>
		/// Generally left null, but can be used as a convenient mechanism 
		/// to have some listeners ignore a subset of notifications for a given type.
		/// Motivating example is window managers existing at different layers in a scene.
		/// </summary>
		/// <value>The group.</value>
		public string group { get; private set; }
		
		/// <summary>
		/// A convenient place to store a param for Notifications that
		/// have just one param.
		/// For notifications with more than one param, use properties.
		/// </summary>
		public object body
		{
			get; set;
		}

		public NotificationReceiverOptions receiverOptions
		{
			get; set;
		}
		
		public bool bodyAsBool
		{
			get {
				return Convert.ToBoolean(this.body);
			}
		}

		public T BodyAs<T>()
		{
			if(this.body is System.IConvertible) { 
				return (T)System.Convert.ChangeType(this.body, typeof(T));
			}
			else {
				return (T)this.body;
			}
		}

		public bool TryGet<T>(string key, out T val)
		{
			object tmp;
			if(m_properties != null && m_properties.TryGetValue(key, out tmp) && tmp is T) {
				val = (T)System.Convert.ChangeType(tmp, typeof(T));
				return true;
			}
			else {
				val = default(T);
				return false;
			}
		}
		
		/// <summary>
		/// Set a key-value param on the Notification
		/// </summary>
		public Notification Set(string key, object val)
		{
			if(m_properties == null) {
				m_properties = new Dictionary<string, object>();
			}
			m_properties[key] = val;
			return this;
		}
		
		/// <summary>
		/// Enumerates all property names set on this Notification
		/// </summary>
		public IEnumerable<string> propertyNames
		{
			get {
				if(m_properties != null) {
					foreach(string key in m_properties.Keys) {
						yield return key;
					}
				}
			}
		}
		
		public override string ToString()
		{
			return "Notification[type: " + this.type + ", body: " + this.body + "]";
		}
		
		private Dictionary<string,object> m_properties;
	}

}
