#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UGUIAnimationToolkit.Editor
{
    struct MenuItem
    {
        public string Path;
        public Type Type;
        public int Order;
    }

    [CustomEditor(typeof(UIButtonAnimator))]
    public class UIButtonAnimatorEditor : UnityEditor.Editor
    {
        // [추가] 모듈 복사를 위한 정적(static) 클립보드 변수
        private static string _clipboardJson;
        private static Type _clipboardType;

        private ReorderableList _hoverList, _clickList;
        private SerializedProperty _hoverSequenceProp, _clickSequenceProp;
        private int _selectedTab = 0;
        private readonly GUIContent[] _tabs = { new GUIContent("Hover"), new GUIContent("Click") };

        private void OnEnable()
        {
            var so = serializedObject;
            _hoverSequenceProp = so.FindProperty("hoverSequence");
            _clickSequenceProp = so.FindProperty("clickSequence");
            _hoverList = CreateList(_hoverSequenceProp.FindPropertyRelative("modules"));
            _clickList = CreateList(_clickSequenceProp.FindPropertyRelative("modules"));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space(5);
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs, GUILayout.Height(25));
            EditorGUILayout.Space(10);
            switch (_selectedTab)
            {
                case 0:
                    DrawSequenceGroup("Hover Animation Settings", _hoverList, _hoverSequenceProp);
                    break;
                case 1:
                    DrawSequenceGroup("Click Animation Settings", _clickList, _clickSequenceProp);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSequenceGroup(string title, ReorderableList list, SerializedProperty sequenceProp)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
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

            // [수정] onAddDropdownCallback에 'Paste as New' 기능 추가
            list.onAddDropdownCallback = (rect, l) =>
            {
                var menu = new GenericMenu();

                // 1. 클립보드에 내용이 있으면 'Paste as New' 메뉴를 최상단에 추가
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

                // ... (이하 카테고리/정렬 기능은 이전과 동일)

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

            // [수정] drawElementCallback의 '...' 메뉴에 'Copy'와 'Paste Values' 기능 추가
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

                    // Copy 기능 추가
                    menu.AddItem(new GUIContent("Copy Values"), false, () =>
                    {
                        var module = element.managedReferenceValue;
                        _clipboardType = module.GetType();
                        _clipboardJson = JsonUtility.ToJson(module);
                        Debug.Log($"{_clipboardType.Name} values copied.");
                    });

                    // Paste Values 기능 추가 (클립보드에 내용이 있고, 타입이 같을 때만 활성화)
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