using UnityEngine;
using System.Collections;

namespace BeatThat.App
{
	/// <summary>
	/// A way to namespace notifications by putting them in 'context' silos.
	/// </summary>
	public interface NotificationContext 
	{
		/// <summary>
		/// Translates the type to one isolated for the context, e.g. 'mytype' => 'myctx-mytype'
		/// </summary>
		string TranslateNotificationType(string notificationType);
	}
}
