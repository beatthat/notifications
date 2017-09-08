using System.Collections.Generic;

namespace BeatThat.App
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
				m_bindings = new List<NotificationBinding>();
			}
			m_bindings.Add(b);
		}

		public void UnbindAll()
		{
			if(m_bindings != null) {
				List<NotificationBinding> tmp = m_bindings;
				m_bindings = null;
				foreach(NotificationBinding b in tmp) {
					b.Unbind();
				}
			}
		}

		private List<NotificationBinding> m_bindings;
	}
}

