using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
//using BeatThat.Editor;

namespace BeatThat.App
{
	public static class NotificationEditorUtils 
	{
		public static string[] GetAllNotificationTypes()
		{
			return EditorUtils.FindStaticValsWithAttrAndValType<NotificationTypeAttribute, string>();

//			if(m_notificationTypes == null) {
//				var noticationTypes = new Dictionary<string, string>();
//				foreach(Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
//					foreach(Type t in a.GetTypes()) {
//						foreach(FieldInfo f in t.GetFields()) {
//							if(f.FieldType == typeof(string)) {
//								foreach(var attr in f.GetCustomAttributes(false)) {
//									if(attr is NotificationTypeAttribute) {
//										string s = f.GetRawConstantValue().ToString();
//										noticationTypes[s] = s;
//									}
//								}
//							}
//						}
//					}
//				}
//
//				m_notificationTypes = new List<string>(noticationTypes.Keys).ToArray();
//
//				System.Array.Sort(m_notificationTypes);
//			}
//			return m_notificationTypes;
		}

//		private static string[] m_notificationTypes;
	}
}
