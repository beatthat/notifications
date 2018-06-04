using UnityEngine;
using System;
using System.Collections.Generic;

namespace BeatThat.App
{
	/// <summary>
	/// A global publish/subscribe singleton that allows very loose coupling 
	/// between notification publishers and notification subscribers.
	/// </summary>
	public class NotificationBus 
	{		
		public static NotificationBus instance
		{
			get {
				if(INSTANCE == null) {
					INSTANCE = new NotificationBus();
				}
				return INSTANCE;
			}
		}

		public void SetContext(NotificationContext ctx)
		{
			this.activeContext = ctx;
		}

		public NotificationContext activeContext
		{
			get; private set;
		}

		/// <summary>
		/// Convenience static access to instance method AddHandler (with single-arg ref-type callback)
		/// </summary>
		/// <param name="type">The type of the Notification</param>
		/// <param name="handler">The callback for when a notification of the given type occurs.</param>
		/// <param name="ownerRef">Used to ensure that the NotificationBus does not hold on top zombie refs 
		/// if a handler is attached to a GameObject and the GameObject gets destroyed.</param>
		/// <typeparam name="T">The type of the callback when notifications occur.</typeparam>
		public static NotificationBinding Add<T>(string type, Action<T> handler, object ownerRef = null) 
		{
			return NotificationBus.instance.AddHandler<T>(type, handler, ownerRef);
		}

		/// <summary>
		/// Convenience static access to instance method AddHandler (with no-arg callback)
		/// </summary>
		/// <param name="type">The type of the Notification</param>
		/// <param name="handler">The callback for when a notification of the given type occurs.</param>
		/// <param name="ownerRef">Used to ensure that the NotificationBus does not hold on top zombie refs 
		/// if a handler is attached to a GameObject and the GameObject gets destroyed.</param>
		public static NotificationBinding Add(string type, Action handler, object ownerRef = null) 
		{
			return NotificationBus.instance.AddHandler(type, handler, ownerRef);
		}
		
		/// <summary>
		/// Add a handler for a given Notification type where the callback function takes a single ref-type param.
		/// The generic type param is the type of the single parameter in the callback.
		/// </summary>
		/// <returns>
		/// The Binding object which can be used to check binding status and safely unbind/remove the handler.
		/// </returns>
		/// <param name='type'>
		/// The type of Notifications desired. Notification sent that match this type will be dispatched
		/// to all registered handlers. Notification types are CASE INSENSITIVE.
		/// </param>
		/// <param name='handler'>
		/// Callback-function handler. 
		/// </param>
		/// <param name='ownerRef'>
		/// Optional gameobject, that should be the owner of the handler function if passed. 
		/// Used to make sure a Notification is never sent to a GameObject/handler that's been destroyed.
		/// </param>
		public NotificationBinding AddHandler<T>(string type, Action<T> handler, object ownerRef = null) 
		{
//#if DEBUG_ENABLED
//			Debug.Log ("[" + Time.time + "] " + GetType() + "::AddHandler '" + type + "': " + handler);
//#endif
			NotificationBinding b;
			if(FindBinding(type, handler, out b)) {
				return b;
			}
			else {
				return AddBinding(new OneArgNotificationBinding<T>(this, type, handler, ToGO(ownerRef)));
			}
		}

		/// <summary>
		/// Add a handler for a given Notification type where the callback function takes no params.
		/// </summary>
		/// <returns>
		/// The Binding object which can be used to check binding status and safely unbind/remove the handler.
		/// </returns>
		/// <param name='type'>
		/// The type of Notifications desired. Notification sent that match this type will be dispatched
		/// to all registered handlers. Notification types are CASE INSENSITIVE.
		/// </param>
		/// <param name='handler'>
		/// Callback-function handler. 
		/// </param>
		/// <param name='ownerRef'>
		/// Optional gameobject, that should be the owner of the handler function if passed. 
		/// Used to make sure a Notification is never sent to a GameObject/handler that's been destroyed.
		/// </param>
		public NotificationBinding AddHandler(string type, Action handler, object ownerRef = null) 
		{
			NotificationBinding b;
			if(FindBinding(type, handler, out b)) {
				return b;
			}
			else {
				return AddBinding(new NoArgNotificationBinding(this, type, handler, ToGO(ownerRef)));
			}
		}

		public void LogBindings()
		{
			using(var tmp = ListPool<string>.Get()) {

				foreach(var noteBindings in m_bindingsByType) {
					tmp.Clear();

					foreach(var b in noteBindings.Value) {
						tmp.Add(b.bindingEndPointInfo);
					}

					Debug.Log ("[" + Time.time + "][" + noteBindings.Key
					           + "]...[" + string.Join(",", tmp.ToArray()) + "]");

				}
			}
		}

		private GameObject ToGO(object o)
		{
			if(o is GameObject) {
				return o as GameObject;
			}
			else if (o is Component && o != null) {
				return (o as Component).gameObject;
			}
			else {
				return null;
			}
		}

		private bool FindBinding(string type, object handler, out NotificationBinding theBinding)
		{
			List<NotificationBindingBase> bindings = BindingsForType(type, false);

			if(bindings != null) {
				foreach(var b in bindings) {
					if(b.isValid && b.ContainsCallback(handler)) {
						theBinding = b;
						return true;
					}
				}
			}

			theBinding = null;
			return false;
		}

		private NotificationBinding AddBinding(NotificationBindingBase b)
		{
			BindingsForType(b.notificationType, true).Add(b);
			return b;
		}

		public static bool Remove(string type, Action handler)
		{
			return NotificationBus.instance.RemoveHandlerObject(type, (object)handler);
		}
		
		public static bool Remove<T>(string type, Action<T> handler)
		{
			return NotificationBus.instance.RemoveHandlerObject(type, (object)handler);
		}

		public bool RemoveHandler(string type, Action handler)
		{
			return NotificationBus.instance.RemoveHandlerObject(type, (object)handler);
		}

		public bool RemoveHandler<T>(string type, Action<T> handler)
		{
			return NotificationBus.instance.RemoveHandlerObject(type, (object)handler);
		}

		/// <summary>
		/// Removes the Notification handler from the list for the given type.
		/// </summary>
		/// <returns>
		/// True if the handler was removed. False if the handler was NOT removed (because it wasn't registered).
		/// </returns>
		private bool RemoveHandlerObject(string type, object handler) 
		{
			List<NotificationBindingBase> bindings = BindingsForType(type, false);
			if(bindings == null) {
				return false;
			}

			for(int i = bindings.Count - 1; i >= 0; i--) {
				if(bindings[i].ContainsCallback(handler)) {

//#if DEBUG_ENABLED
//					Debug.Log ("[" + Time.time + "] " + GetType() + "::RemoveHandler '" 
//					           + type + "': " + handler + " FOUND AND REMOVED");
//#endif

					bindings[i].Unbind();
					return true;
				}
			}

//#if DEBUG_ENABLED
//			Debug.Log ("[" + Time.time + "] " + GetType() + "::RemoveHandler '" 
//			           + type + "': " + handler + " NOT FOUND");
//#endif

			return false;
		}

		/// <summary>
		/// Convenience static access to instance method Send.
		/// </summary>
		public static void Send(string type, NotificationReceiverOptions opts = NotificationReceiverOptions.RequireReceiver) 
		{
			NotificationBus.instance.SendNotification(type, opts);
		}

		/// <summary>
		// Convenience static access to instance method Send.
		/// </summary>
		public static void Send<T>(string type, T body, NotificationReceiverOptions opts = NotificationReceiverOptions.RequireReceiver) 
		{
			NotificationBus.instance.SendNotificationWithBody<T>(type, body, opts);
		}

		/// <summary>
		// Convenience static access to instance method Send.
		/// </summary>
		public static void SendWBody<T>(string type, T body, NotificationReceiverOptions opts = NotificationReceiverOptions.RequireReceiver) 
		{
			NotificationBus.instance.SendNotificationWithBody<T>(type, body, opts);
		}
			
		
		/// <summary>
		/// Convenience static access to instance method SendNotification.
		/// </summary>
		public void SendNotification(string type, NotificationReceiverOptions opts = NotificationReceiverOptions.RequireReceiver) 
		{
			bool anyBindings = false;
			
			List<NotificationBindingBase> bindings = BindingsForType(type, false);
			if(bindings != null) {
				for(int i = bindings.Count - 1; i >= 0; i--) {
					if(bindings[i] is NoArgNotificationBinding) {
						(bindings[i] as NoArgNotificationBinding).Send();
						anyBindings = true;
						continue;
					}

					if(bindings[i].GetType() == typeof(OneArgNotificationBinding<Notification>)) {
						(bindings[i] as OneArgNotificationBinding<Notification>).SendBody(new Notification(type, null, opts));
						anyBindings = true;
						continue;
					}

					Debug.LogWarning("[" + Time.frameCount + "] '" + type + "' sent with no args and encountered a binding that expects args: " + bindings[i]);
				}
			}
			
			if(!anyBindings && opts == NotificationReceiverOptions.RequireReceiver) {
				#if BT_DEBUG_UNSTRIP || UNITY_EDITOR
				Debug.LogError("[" + Time.frameCount + "] No listeners for notification sent with type '" + type + "'");
				#endif
			}
		}

		public void SendNotificationWithBody<T>(string type, T body, NotificationReceiverOptions opts = NotificationReceiverOptions.RequireReceiver) 
		{
			bool anyBindings = false;
			
			List<NotificationBindingBase> bindings = BindingsForType(type, false);
			if(bindings != null && bindings.Count > 0) {
				for(int i = bindings.Count - 1; i >= 0; i--) {
					
					i = (i < bindings.Count) ? i : bindings.Count - 1;
					if (i < 0) {
						break;
					}

					if(bindings[i] is NoArgNotificationBinding) {
						(bindings[i] as NoArgNotificationBinding).Send();
						anyBindings = true;
						continue;
					}

					if(bindings[i].GetType() == typeof(OneArgNotificationBinding<Notification>) && !(body is Notification)) {
						(bindings[i] as OneArgNotificationBinding<Notification>).SendBody(new Notification(type, body, opts));
						anyBindings = true;
						continue;
					}

					var b = bindings[i] as OneArgNotificationBinding<T>;
					if(b != null) {
						b.SendBody(body);
						anyBindings = true;
						continue;
					}
						
					anyBindings |= bindings[i].SendObject((object)body);
				}
			}
			
			if(!anyBindings && opts == NotificationReceiverOptions.RequireReceiver) {
				#if BT_DEBUG_UNSTRIP || UNITY_EDITOR
				Debug.LogError("[" + Time.frameCount + "] No listeners for notification sent with type '" + type + "'");
				#endif
			}
		}

		private string EffectiveType(string type)
		{
			return this.activeContext != null ? this.activeContext.TranslateNotificationType (type) : type;
		}
		
		private List<NotificationBindingBase> BindingsForType(string type, bool create) 
		{
			string effectiveType = EffectiveType(type);

			ListPoolList<NotificationBindingBase> bindings;
		
			if(m_bindingsByType.TryGetValue(effectiveType, out bindings)) {
				return bindings;
			}
			else if(create) {
				bindings = ListPool<NotificationBindingBase>.Get();
				m_bindingsByType[effectiveType] = bindings;
				return bindings;
			}
			else {
				return null;
			}
		}

		protected bool RemoveBinding(string type, NotificationBindingBase b)
		{
			string effectiveType = EffectiveType(type);

			bool didRemove = false;
			ListPoolList<NotificationBindingBase> bindings;
			if(m_bindingsByType.TryGetValue(effectiveType, out bindings) && bindings != null) {
				bindings.Remove(b);
			
				if(bindings.Count == 0) {
					didRemove = m_bindingsByType.Remove(effectiveType);
					bindings.Dispose();
				}
			}

//#if DEBUG_ENABLED
//			if(didRemove) {
//				Debug.Log ("[" + Time.time + "] " + GetType() + "::RemoveBinding '" 
//				           + type + "' FOUND AND REMOVED BINDING");
//			}
//			else {
//				Debug.Log ("[" + Time.time + "] " + GetType() + "::RemoveBinding '" 
//				           + type + "' NOT FOUND");
//			}
//#endif

			return didRemove;
		}

		protected class NoArgNotificationBinding : NotificationBindingBase
		{
			public NoArgNotificationBinding(NotificationBus owner, string type, 
			                                Action callback, GameObject go) : base(owner, type, go)
			{
				m_callback = callback;
			}
			
			override public bool ContainsCallback(object callback)
			{
				return callback.Equals(m_callback);
			}

			public void Send()
			{
				if(!this.isBound) {
					return;
				}
				
				if(!this.isValid) {
					Unbind();
					return;
				}

				m_callback();
			}

			override public bool SendObject(object body)
			{
				throw new NotSupportedException();
			}

			private Action m_callback;
		}

		protected class OneArgNotificationBinding<T> : NotificationBindingBase
		{
			public OneArgNotificationBinding(NotificationBus owner, string type, 
			                                Action<T> callback, GameObject go) : base(owner, type, go)
			{
				m_callback = callback;
			}

			override public bool ContainsCallback(object callback)
			{
				return callback.Equals(m_callback);
			}

			public void SendBody(T body)
			{
				if(!this.isBound) {
					return;
				}
				
				if(!this.isValid) {
					Unbind();
					return;
				}

				m_callback(body);
			}

			override public bool SendObject(object body)
			{
				if(!this.isBound) {
					return false;
				}

				if(!this.isValid) {
					Unbind();
					return false;
				}

				T val;
				if((typeof(T) == typeof(Notification))) {
					val = (T)Convert.ChangeType(body, typeof(T));
				}
				else {
					if(body is T) {
						val = (T)body;
					}
					else if(body is IConvertible) {
						try {
							val = (T)Convert.ChangeType(body, typeof(T));
						}
						#pragma warning disable 0168
						catch(Exception e) {
							#if UNITY_EDITOR || BT_DEBUG_UNSTRIP
							Debug.LogError("[" + Time.frameCount + "] invalid cast to listener type " + typeof(T) 
								+ " for notification body with type " 
								+ ((body != null)? body.GetType().ToString(): "null"));
							#endif
							return false;
						}
						#pragma warning restore 0168
					}
					else {
						try {
							val = (T)body;
						}
						#pragma warning disable 0168
						catch(Exception e) {
							#if UNITY_EDITOR || BT_DEBUG_UNSTRIP
							Debug.LogError("[" + Time.frameCount + "] invalid cast to listener type " + typeof(T) 
								+ " for notification body with type " 
								+ ((body != null)? body.GetType().ToString(): "null"));
							#endif
							return false;
						}
						#pragma warning restore 0168
					}
				}

				m_callback(val);
				return true;
			}

			private readonly Action<T> m_callback;
		}

		protected abstract class NotificationBindingBase : NotificationBinding
		{
			public NotificationBindingBase(NotificationBus owner, string type, GameObject go)
			{
				m_owner = owner;
				this.notificationType = type;
				m_gameObject = go;
				m_checkGameObjectBeforeSend = (go != null);
				this.isBound = true;
			}
			
			public abstract bool ContainsCallback(object callback);

			/// <summary>
			/// Fallback for cases where type of sent notification does not exactly match
			/// type expected by binding.
			/// Tries to convert.
			/// </summary>
			/// <param name="body">Body.</param>
			abstract public bool SendObject(object body);

			public string bindingEndPointInfo
			{
				get {
					return (m_gameObject != null)? m_gameObject.name: "null";
				}
			}
			
			public bool isValid
			{
				get {
					return m_gameObject != null || !m_checkGameObjectBeforeSend;
				}
			}
			
			public void Unbind()
			{
				if(this.isBound) {
					m_owner.RemoveBinding(this.notificationType, this);
					this.isBound = false;
				}
			}
			
			public string notificationType 
			{
				get; private set;
			}
			
			public bool isBound 
			{
				get; private set;
			}
			
			private NotificationBus m_owner;
			private GameObject m_gameObject;
			private bool m_checkGameObjectBeforeSend;
		}

		private readonly Dictionary<string, ListPoolList<NotificationBindingBase>> m_bindingsByType 
		= new Dictionary<string, ListPoolList<NotificationBindingBase>>();

		private static NotificationBus INSTANCE;
	}
}

