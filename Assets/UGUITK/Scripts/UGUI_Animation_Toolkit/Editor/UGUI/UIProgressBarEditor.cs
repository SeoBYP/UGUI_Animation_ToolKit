using System;
using System.Linq;
using System.Reflection;
using UGUIAnimationToolkit;
using UGUIAnimationToolkit.Editor;
using UGUIAnimationToolkit.ProgressBar;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using MenuItem = UGUIAnimationToolkit.Editor.MenuItem;

namespace UGUIAnimationToolkit.ProgressBar.Editor
{
    [CustomEditor(typeof(UIProgressBar))]
    [CanEditMultipleObjects]
    public class UIProgressBarEditor : UnityEditor.Editor
    {
        // --- 프로퍼티 ---
        private SerializedProperty m_FillImageProperty;
        private SerializedProperty m_MinValueProperty;
        private SerializedProperty m_MaxValueProperty;
        private SerializedProperty m_ValueProperty;
        private SerializedProperty m_DirectionProperty;
        private SerializedProperty m_OnValueChangedProperty;

        // --- 애니메이션 시퀀스 프로퍼티 ---
        private ReorderableList _animationList;
        private SerializedProperty _animationSequenceProp;

        private void OnEnable()
        {
            var so = serializedObject;

            // 프로그레스 바의 모든 직렬화된 필드를 찾아옵니다.
            m_FillImageProperty = so.FindProperty("m_FillImage");
            m_MinValueProperty = so.FindProperty("m_MinValue");
            m_MaxValueProperty = so.FindProperty("m_MaxValue");
            m_ValueProperty = so.FindProperty("m_Value");
            m_DirectionProperty = so.FindProperty("m_Direction");
            m_OnValueChangedProperty = so.FindProperty("m_OnValueChanged");

            _animationSequenceProp = so.FindProperty("m_AnimationSequence");
            if (_animationSequenceProp != null)
            {
                _animationList = CreateList(_animationSequenceProp.FindPropertyRelative("modules"));
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Target Graphics", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_FillImageProperty);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Value Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_MinValueProperty);
            EditorGUILayout.PropertyField(m_MaxValueProperty);

            // 값을 직접 수정하는 슬라이더 UI 추가
            float min = m_MinValueProperty.floatValue;
            float max = m_MaxValueProperty.floatValue;
            float val = m_ValueProperty.floatValue;
            EditorGUI.BeginChangeCheck();
            val = EditorGUILayout.Slider("Value", val, min, max);
            if (EditorGUI.EndChangeCheck())
            {
                m_ValueProperty.floatValue = val;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(m_DirectionProperty);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Animation Sequence", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (_animationList != null)
            {
                _animationList.DoLayoutList();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Value Changed Event", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_OnValueChangedProperty);

            serializedObject.ApplyModifiedProperties();
        }

        // UGUIButtonEditor의 CreateList 로직을 그대로 가져와서 ProgressBar 모듈에 맞게 수정합니다.
        private ReorderableList CreateList(SerializedProperty modulesProp)
        {
            if (modulesProp == null) return null;
            var list = new ReorderableList(serializedObject, modulesProp, true, true, true, true);

            list.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, "ProgressBar Animation Modules", EditorStyles.boldLabel);
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
                // [핵심 수정] ProgressBarAnimationModule을 찾도록 변경
                foreach (var type in TypeCache.GetTypesDerivedFrom<ProgressBarAnimationModule>())
                {
                    if (type.IsAbstract) continue;
                    var typeName = type.Name.Replace("Module", "");
                    // [핵심 수정] 프로그레스 바용 ModuleCategoryAttribute를 찾도록 변경
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