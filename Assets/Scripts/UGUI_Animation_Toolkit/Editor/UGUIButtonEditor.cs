#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UGUIAnimationToolkit.Editor
{
    // 메뉴 항목 정보를 임시로 저장할 구조체

    [CustomEditor(typeof(UGUIButton), true)]
    [CanEditMultipleObjects]
    public class UGUIButtonEditor : UnityEditor.Editor
    {
        // --- 클립보드 ---
        private static string _clipboardJson;
        private static Type _clipboardType;

        // --- 프로퍼티 ---
        private SerializedProperty m_InteractableProperty;
        private SerializedProperty m_OnClickProperty;
        private SerializedProperty m_ThrottleDurationProperty;
        private SerializedProperty m_DebounceDurationProperty;

        // --- 애니메이션 시퀀스 프로퍼티 ---
        private ReorderableList _hoverList, _clickList;
        private SerializedProperty _hoverSequenceProp, _clickSequenceProp;
        private int _selectedTab = 0;
        private readonly GUIContent[] _tabs = { new GUIContent("Hover Animation"), new GUIContent("Click Animation") };

        private void OnEnable()
        {
            var so = serializedObject;
            m_InteractableProperty = so.FindProperty("m_Interactable");
            m_OnClickProperty = so.FindProperty("m_OnClick");
            m_ThrottleDurationProperty = so.FindProperty("m_ThrottleDuration");
            m_DebounceDurationProperty = so.FindProperty("m_DebounceDuration");
            _hoverSequenceProp = so.FindProperty("hoverSequence");
            _clickSequenceProp = so.FindProperty("clickSequence");

            _hoverList = CreateList(_hoverSequenceProp.FindPropertyRelative("modules"));
            _clickList = CreateList(_clickSequenceProp.FindPropertyRelative("modules"));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_InteractableProperty);
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Button Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ThrottleDurationProperty);
            EditorGUILayout.PropertyField(m_DebounceDurationProperty);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Animation Sequence", EditorStyles.boldLabel);
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs, GUILayout.Height(25));
            EditorGUILayout.Space(5);
            switch (_selectedTab)
            {
                case 0: DrawSequenceGroup(_hoverList); break;
                case 1: DrawSequenceGroup(_clickList); break;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Click Event", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_OnClickProperty);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSequenceGroup(ReorderableList list)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (list != null)
            {
                list.DoLayoutList();
            }

            EditorGUILayout.EndVertical();
        }

        private ReorderableList CreateList(SerializedProperty modulesProp)
        {
            if (modulesProp == null) return null;
            var list = new ReorderableList(serializedObject, modulesProp, true, true, true, true);

            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Modules", EditorStyles.boldLabel);
            list.elementHeightCallback = index =>
            {
                if (index < 0 || index >= modulesProp.arraySize) return EditorGUIUtility.singleLineHeight;
                var element = modulesProp.GetArrayElementAtIndex(index);
                if (element.managedReferenceValue == null) return EditorGUIUtility.singleLineHeight * 2;
                if (!element.isExpanded) return EditorGUIUtility.singleLineHeight + 4;
                return EditorGUI.GetPropertyHeight(element, true) + 10;
            };

            list.onAddDropdownCallback = (rect, l) =>
            {
                var menu = new GenericMenu();
                if (_clipboardJson != null && _clipboardType != null)
                {
                    var pasteName = _clipboardType.Name.Replace("Module", "");
                    menu.AddItem(new GUIContent($"Paste As New ({pasteName})"), false, () =>
                    {
                        var newModule = Activator.CreateInstance(_clipboardType);
                        JsonUtility.FromJsonOverwrite(_clipboardJson, newModule);
                        modulesProp.arraySize++;
                        var element = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
                        element.managedReferenceValue = newModule;
                        element.isExpanded = true;
                        serializedObject.ApplyModifiedProperties();
                    });
                    menu.AddSeparator("");
                }

                var menuItems = new System.Collections.Generic.List<MenuItem>();
                foreach (var type in TypeCache.GetTypesDerivedFrom<ButtonAnimationModule>())
                {
                    if (type.IsAbstract) continue;
                    var typeName = type.Name.Replace("Module", "");
                    var categoryAttribute = type.GetCustomAttribute<ModuleCategoryAttribute>();
                    var path = categoryAttribute?.Path != null ? $"{categoryAttribute.Path}/{typeName}" : typeName;
                    var order = categoryAttribute?.Order ?? int.MaxValue;
                    menuItems.Add(new MenuItem { Path = path, Type = type, Order = order });
                }

                var sortedItems = menuItems.OrderBy(item => item.Order).ThenBy(item => item.Path);
                foreach (var item in sortedItems)
                {
                    menu.AddItem(new GUIContent(item.Path), false, () =>
                    {
                        modulesProp.arraySize++;
                        var element = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
                        var moduleType = item.Type;
                        element.managedReferenceValue = Activator.CreateInstance(moduleType);
                        element.isExpanded = true;
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = modulesProp.GetArrayElementAtIndex(index);
                rect.y += 2;

                if (element.managedReferenceValue == null)
                {
                    EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight * 2 - 4),
                        "Module is null or missing. Please remove it.", MessageType.Warning);
                    return;
                }

                var headerRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.DrawRect(headerRect, new Color(0.18f, 0.18f, 0.18f));

                var enableProp = element.FindPropertyRelative("Enable");
                var enableRect = new Rect(headerRect.x + 5, headerRect.y, 15, headerRect.height);
                enableProp.boolValue = EditorGUI.Toggle(enableRect, enableProp.boolValue);

                string typeName = element.managedReferenceFullTypename.Split('.').Last().Replace("Module", "");
                var labelRect = new Rect(enableRect.xMax, headerRect.y, headerRect.width - 60, headerRect.height);
                if (GUI.Button(labelRect, typeName, EditorStyles.boldLabel))
                {
                    element.isExpanded = !element.isExpanded;
                }

                var menuRect = new Rect(headerRect.xMax - 25, headerRect.y, 25, headerRect.height);
                if (EditorGUI.DropdownButton(menuRect, new GUIContent("..."), FocusType.Passive, GUI.skin.label))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Copy Values"), false, () =>
                    {
                        var module = element.managedReferenceValue;
                        _clipboardType = module.GetType();
                        _clipboardJson = JsonUtility.ToJson(module);
                    });

                    if (_clipboardJson != null && _clipboardType == element.managedReferenceValue.GetType())
                    {
                        menu.AddItem(new GUIContent("Paste Values"), false, () =>
                        {
                            JsonUtility.FromJsonOverwrite(_clipboardJson, element.managedReferenceValue);
                            serializedObject.ApplyModifiedProperties();
                        });
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("Paste Values"));
                    }

                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Remove"), false, () => modulesProp.DeleteArrayElementAtIndex(index));
                    menu.ShowAsContext();
                }

                if (element.isExpanded)
                {
                    var contentRect = new Rect(rect.x, headerRect.yMax + 4, rect.width, 0);
                    var prop = element.Copy();
                    var endProp = prop.GetEndProperty();
                    prop.NextVisible(true);
                    EditorGUI.indentLevel++;
                    while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, endProp))
                    {
                        contentRect.height = EditorGUI.GetPropertyHeight(prop, true);
                        EditorGUI.PropertyField(contentRect, prop, true);
                        contentRect.y += contentRect.height + EditorGUIUtility.standardVerticalSpacing;
                    }

                    EditorGUI.indentLevel--;
                }
            };
            return list;
        }
    }
}
#endif