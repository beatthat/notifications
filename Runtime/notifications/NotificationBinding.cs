using BeatThat.Bindings;
namespace BeatThat.Notifications
{
    public interface NotificationBinding : Binding
	{
		string notificationType { get; }
	}
}

