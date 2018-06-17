using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cordonez.Modules.FindMembers.Editor
{
	public class FindMembersWindow : EditorWindow
	{
		private string m_componentAssembly = "";
		private string m_componentTypeSearched = "";
		private string m_fieldTypeSearched = "";
		private Vector2 m_scrollPos;
		private bool m_searchOnlyNulls;
		private bool m_showComponents;

		[MenuItem("Tools/Find members")]
		public static void CreateWindow()
		{
			FindMembersWindow window = (FindMembersWindow) GetWindow(typeof(FindMembersWindow));
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

			int scenesCount = SceneManager.sceneCount;
			List<GameObject> allGameobjects = new List<GameObject>();
			for (int i = 0; i < scenesCount; i++)
			{
				allGameobjects.AddRange(SceneManager.GetSceneAt(i).GetRootGameObjects().ToList());
			}

			List<Component> componentsCollection = new List<Component>();
			foreach (GameObject gameObject in allGameobjects)
			{
				Component[] components = gameObject.GetComponentsInChildren(componentType, true);
				componentsCollection.AddRange(components.Where(_component => _component != null));
			}

			m_showComponents = EditorGUILayout.Foldout(m_showComponents, string.Format("Found {0} components", componentsCollection.Count), true);
			if (m_showComponents)
			{
				foreach (Component component in componentsCollection)
				{
					GUILayout.Label("\t" + component.name);
				}
			}

			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			m_scrollPos = EditorGUILayout.BeginScrollView(m_scrollPos);
			foreach (Component component in componentsCollection)
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
			bool result = _field.FieldType.ToString() == _type;
			result |= _field.FieldType.BaseType != null && _field.FieldType.BaseType.FullName != null && _field.FieldType.BaseType.FullName.Contains(_type);
			if (m_searchOnlyNulls)
			{
				result &= _field.GetValue(_component) == null || _field.GetValue(_component).Equals(null);
			}

			return result;
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