using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Cordonez.Modules.FindMembers.Editor
{
	public class FindMembersWindow : EditorWindow
	{
		private string m_componentAssembly = "";
		private string m_componentTypeSearched = "";
		private string m_fieldTypeSearched = "";
		private bool m_searchOnlyNulls;
		private bool m_showComponents;
		private Vector2 m_scrollPos;

		[MenuItem("Tools/Find members")]
		public static void CreateWindow()
		{
			FindMembersWindow window = (FindMembersWindow)GetWindow(typeof(FindMembersWindow));
			window.Show();
		}

		private void OnGUI()
		{
			DrawHeader();
			if (string.IsNullOrEmpty(m_componentTypeSearched))
			{
				return;
			}

			UpdateResults();
		}


		private void UpdateResults()
		{
			Type componentType = FindType(m_componentTypeSearched, m_componentAssembly);
			if (componentType == null)
			{
				EditorGUILayout.LabelField("Error: Component type not found");
				return;
			}

			var componentsCollection = FindObjectsOfType(componentType);
			m_showComponents = EditorGUILayout.Foldout(m_showComponents,
				string.Format("Found {0} components", componentsCollection.Length), true);
			if (m_showComponents)
			{
				foreach (var component in componentsCollection)
				{
					GUILayout.Label("\t" + component.name);
				}
			}

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
			foreach (var component in componentsCollection)
			{
				FieldInfo[] fields = component.GetType().GetFields();
				foreach (FieldInfo field in fields)
				{
					if (ShouldFieldBeDisplayed(component, field, m_fieldTypeSearched))
					{
						GUILayout.Label("Field found: " + field + " in object " + component);
						EditorGUILayout.ObjectField("Object: ", component, typeof(UnityEngine.Object), true);
						EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
					}
				}
			}

			EditorGUILayout.EndScrollView();
		}

		private bool ShouldFieldBeDisplayed(object _component, FieldInfo _field, string _type)
		{
			return (_field.FieldType.ToString() == _type || _field.FieldType.BaseType.FullName.Contains(_type))
				   && (!m_searchOnlyNulls || _field.GetValue(_component) == null || _field.GetValue(_component).Equals(null));
		}

		private void DrawHeader()
		{
			GUILayout.Label("Description:");
			GUILayout.TextArea("This tool will scan the open scenes for the component type specified and for every one found will search for fields of the type specified.");
			GUILayout.Label("Assembly Info:");
			GUILayout.TextArea(
				"Leave blank to search in the currently executing assembly and Mscorlib (usually all your custom classes will be in the current one so leave it empty). All the Unity components(Lights,Gameobjects, Animator) are in the 'UnityEngine' assembly. The type must contatin the full name(namespace included)." +
				"\n\tA example would be " +
				"\n\t\tComponent Assembly:UnityEngine" +
				"\n\t\tComponent Type:UnityEngine.MonoBehaviour" +
				"\n\t\tField Type:UnityEngine.Camera" +
				"\nThis will look for any MonoBehaviours that contains a Camera field." +
				"\nThe field type searchs if the full name of the type contains the type defined to be able to look on base classes");
			GUILayout.Label("Component assembly:");
			m_componentAssembly = GUILayout.TextField(m_componentAssembly);
			GUILayout.Label("Component Type:");
			m_componentTypeSearched = GUILayout.TextField(m_componentTypeSearched);
			GUILayout.Label("Field Type:");
			m_fieldTypeSearched = GUILayout.TextField(m_fieldTypeSearched);
			m_searchOnlyNulls = GUILayout.Toggle(m_searchOnlyNulls, "Search only null fields");
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			GUILayout.Label("RESULTS:");
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		}

		private static Type FindType(string _type, string _assembly)
		{
			return Type.GetType(_type + (string.IsNullOrEmpty(_assembly) ? "" : "," + _assembly));
		}
	}
}