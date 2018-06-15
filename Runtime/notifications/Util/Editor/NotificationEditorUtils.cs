namespace BeatThat.Notifications
{
    public static class NotificationEditorUtils 
	{
		public static string[] GetAllNotificationTypes()
		{
			return TypeUtils.FindStaticValsWithAttrAndValType<NotificationTypeAttribute, string>();
		}
	}
}

