using BeatThat.Pools;

namespace BeatThat.Notifications
{
    /// <summary>
    /// Utility class manages a set of notification bindings
    /// and provides a safe single point where all bindings can be unbound.
    /// </summary>
    public class NotificationBindings
	{
		public void Add(NotificationBinding b)
		{
			if(m_bindings == null) {
				m_bindings = ListPool<NotificationBinding>.Get();
			}
			m_bindings.Add(b);
		}

		public void UnbindAll()
		{
			if(m_bindings != null) {
				foreach(NotificationBinding b in m_bindings) {
					b.Unbind();
				}
				m_bindings.Dispose ();
				m_bindings = null;
			}
		}

		private ListPoolList<NotificationBinding> m_bindings;
	}
}



