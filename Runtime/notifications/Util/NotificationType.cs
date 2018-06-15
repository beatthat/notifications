namespace BeatThat.Notifications
{
    /// <summary>
    /// Wrapper class for a notification type, which is really just a string.
    /// The reason to use this class, is to enable a custom propery drawer in unity editor
    /// which can find all notification types marked with the attribute NotificationType
    /// and make them pickable in a drop down.
    /// </summary>
    [System.Serializable]
	public class NotificationType 
	{
		public string m_notificationType;

		public NotificationType() {}

		public NotificationType(string n) 
		{
			this.notificationType = n;
		}

		// Analysis disable ConvertToAutoProperty
		public string notificationType 
		// Analysis restore ConvertToAutoProperty
		{
			get {
				return m_notificationType;
			}
			set {
				m_notificationType = value;
			}
		}

		public void Send()
		{
			NotificationBus.Send(this.notificationType);
		}
	}
}

