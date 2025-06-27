#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UGUIAnimationToolkit.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

// 네임스페이스를 Toggle.Editor로 명확히 하는 것이 좋습니다.
namespace UGUIAnimationToolkit.Toggle.Editor
{
    // UGUIButtonEditor와 동일한 MenuItem 구조체를 사용합니다.
    internal struct MenuItem
    {
        public string Path;
        public Type Type;
        public int Order;
    }

    [CustomEditor(typeof(UGUIToggle), true)]
    [CanEditMultipleObjects]
    public class UGUIToggleEditor : UnityEditor.Editor
    {
        // --- 프로퍼티 ---
        private SerializedProperty m_InteractableProperty;
        private SerializedProperty m_IsOnProperty;
        private SerializedProperty m_GraphicProperty;
        private SerializedProperty m_BackgroundGraphicProperty;
        private SerializedProperty m_GroupProperty;
        private SerializedProperty m_OnValueChangedProperty;

        // --- 애니메이션 시퀀스 프로퍼티 ---
        private ReorderableList _onList, _offList;
        private SerializedProperty _onSequenceProp, _offSequenceProp;
        private int _selectedTab = 0;
        private readonly GUIContent[] _tabs = { new GUIContent("On Animation"), new GUIContent("Off Animation") };

        private void OnEnable()
        {
            var so = serializedObject;

            // 토글의 모든 직렬화된 필드를 찾아옵니다.
            m_InteractableProperty = so.FindProperty("m_Interactable");
            m_IsOnProperty = so.FindProperty("m_IsOn");
            m_GraphicProperty = so.FindProperty("graphic");
            m_BackgroundGraphicProperty = so.FindProperty("backgroundGraphic");
            m_GroupProperty = so.FindProperty("m_Group");
            m_OnValueChangedProperty = so.FindProperty("onValueChanged");

            _onSequenceProp = so.FindProperty("onSequence");
            _offSequenceProp = so.FindProperty("offSequence");

            _onList = CreateList(_onSequenceProp.FindPropertyRelative("modules"));
            _offList = CreateList(_offSequenceProp.FindPropertyRelative("modules"));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 1. 토글의 기본 설정 필드를 그립니다.
            EditorGUILayout.LabelField("Toggle Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_InteractableProperty);
            EditorGUILayout.PropertyField(m_IsOnProperty);
            EditorGUILayout.PropertyField(m_GraphicProperty, new GUIContent("Checkmark Graphic"));
            EditorGUILayout.PropertyField(m_BackgroundGraphicProperty, new GUIContent("Background Graphic"));
            EditorGUILayout.PropertyField(m_GroupProperty);

            EditorGUILayout.Space(10);

            // 2. 탭 기반 애니메이션 에디터를 그립니다.
            EditorGUILayout.LabelField("Animation Sequences", EditorStyles.boldLabel);
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs, GUILayout.Height(25));
            EditorGUILayout.Space(5);
            switch (_selectedTab)
            {
                case 0: DrawSequenceGroup(_onList); break;
                case 1: DrawSequenceGroup(_offList); break;
            }

            EditorGUILayout.Space(10);

            // 3. On Value Changed 이벤트를 그립니다.
            EditorGUILayout.LabelField("Value Changed Event", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_OnValueChangedProperty);

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

        // UGUIButtonEditor의 CreateList 로직을 그대로 가져와서
        // ButtonAnimationModule -> ToggleAnimationModule로만 변경합니다.
        private ReorderableList CreateList(SerializedProperty modulesProp)
        {
            if (modulesProp == null) return null;
            var list = new ReorderableList(serializedObject, modulesProp, true, true, true, true);

            list.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, "Toggle Animation Modules", EditorStyles.boldLabel);
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

                // ModuleClipboard를 사용하여 복붙 기능을 구현합니다.
                if (ModuleClipboard.ClipboardJson != null && ModuleClipboard.ClipboardType != null)
                {
                    var pasteName = ModuleClipboard.ClipboardType.Name.Replace("Module", "");
                    menu.AddItem(new GUIContent($"Paste As New ({pasteName})"), false, () =>
                    {
                        var newModule = Activator.CreateInstance(ModuleClipboard.ClipboardType);
                        JsonUtility.FromJsonOverwrite(ModuleClipboard.ClipboardJson, newModule);
                        modulesProp.arraySize++;
                        var element = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
                        element.managedReferenceValue = newModule;
                        element.isExpanded = true;
                        serializedObject.ApplyModifiedProperties();
                    });
                    menu.AddSeparator("");
                }

                var menuItems = new System.Collections.Generic.List<MenuItem>();
                // [핵심 수정] ButtonAnimationModule -> ToggleAnimationModule로 변경
                foreach (var type in TypeCache.GetTypesDerivedFrom<ToggleAnimationModule>())
                {
                    if (type.IsAbstract) continue;
                    var typeName = type.Name.Replace("Module", "");
                    // 토글 모듈용 카테고리 어트리뷰트도 필요합니다.
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
                        ModuleClipboard.ClipboardType = module.GetType();
                        ModuleClipboard.ClipboardJson = JsonUtility.ToJson(module);
                    });

                    if (ModuleClipboard.ClipboardJson != null &&
                        ModuleClipboard.ClipboardType == element.managedReferenceValue.GetType())
                    {
                        menu.AddItem(new GUIContent("Paste Values"), false, () =>
                        {
                            JsonUtility.FromJsonOverwrite(ModuleClipboard.ClipboardJson, element.managedReferenceValue);
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