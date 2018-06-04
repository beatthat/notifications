using UnityEditor;
using UnityEngine;

using System.Collections.Generic;

//using BeatThat.App.Editor;
//using BeatThat.App;

namespace BeatThat.App
{
	[CustomPropertyDrawer(typeof(NotificationType))]
	public class NotificationTypePropertyDrawer : PropertyDrawer
	{

		private string[] notificationTypesSorted {
			get {
				if(m_notificationTypesSorted == null) {
					var tmp = new List<string>(NotificationEditorUtils.GetAllNotificationTypes());
					tmp.Sort(); // so we can binary search
					tmp.Insert(0, "[none]"); // need a way to pick 'no notification type'
					m_notificationTypesSorted = tmp.ToArray();
				}
				return m_notificationTypesSorted;
			}
		}

		private int GetNotificationTypeArrayIndex(SerializedProperty nTypeProp)
		{
			if(string.IsNullOrEmpty(nTypeProp.stringValue)) {
				return -1;
			}

			var ix = System.Array.BinarySearch(this.notificationTypesSorted, nTypeProp.stringValue);
			return ix >= 0? ix: -1;
		}

		override public float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var baseHeight = base.GetPropertyHeight(property, label);

			SerializedProperty nTypeProp =  property.FindPropertyRelative("m_notificationType");

			if(string.IsNullOrEmpty(nTypeProp.stringValue)) {
				return baseHeight;
			}

			// if the prop is set, but NotificationType can't be found, leave room for error message
			return GetNotificationTypeArrayIndex(nTypeProp) >= 0 ? baseHeight: baseHeight * 4;
		}


		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			
			// Draw label
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			SerializedProperty nTypeProp =  property.FindPropertyRelative("m_notificationType");
			
			// Don't make child fields be indented
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var baseHeight = base.GetPropertyHeight(property, label);

			var curRect = new Rect(position.x, position.y, position.width, baseHeight); // position.height);

			var nTypesSorted = this.notificationTypesSorted;

			var isPropSet = !string.IsNullOrEmpty(nTypeProp.stringValue);
			int oldIx = -1;
			if(isPropSet) {
				oldIx = System.Array.BinarySearch(nTypesSorted, nTypeProp.stringValue);
			}

			int newIx = EditorGUI.Popup(curRect, oldIx, nTypesSorted);
			if(newIx != oldIx && (newIx >= 0)) {
				nTypeProp.stringValue = nTypesSorted[newIx];
			}
			else if(isPropSet && oldIx < 0) { // we have a serialized value that doesn't match any NotificationType in the list...
				var style = new GUIStyle(EditorStyles.label);
				style.normal.textColor = Color.red;
				GUI.contentColor = Color.red;

				curRect.y += baseHeight;
				EditorGUI.LabelField(curRect, new GUIContent("Serialized value: '" + nTypeProp.stringValue + "'"), style);
				curRect.y += baseHeight;
				EditorGUI.LabelField(curRect, new GUIContent("Not found. Changed or not longer used?"), style);
			}
			
			// Set indent back to what it was
			EditorGUI.indentLevel = indent;
			
			EditorGUI.EndProperty();
		}

		
		private string[] m_notificationTypesSorted;
	}

	
}
