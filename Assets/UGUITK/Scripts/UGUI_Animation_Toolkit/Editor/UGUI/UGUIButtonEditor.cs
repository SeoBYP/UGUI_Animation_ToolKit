#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UGUIAnimationToolkit.Core; // Dictionary 사용을 위해 추가
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UGUIAnimationToolkit.Editor
{
    [CustomEditor(typeof(UGUIButton), true)]
    [CanEditMultipleObjects]
    public class UGUIButtonEditor : UnityEditor.Editor
    {
        // --- 프로퍼티 ---
        private SerializedProperty m_InteractableProperty;
        private SerializedProperty m_OnClickProperty;
        private SerializedProperty m_ThrottleDurationProperty;
        private SerializedProperty m_DebounceDurationProperty;
        private SerializedProperty m_AnimatorProperty;

        // --- UI 상태 ---
        private int _selectedTab = 0;
        private GUIContent[] _tabs;

        private readonly Dictionary<string, ReorderableList> _moduleListsCache =
            new Dictionary<string, ReorderableList>();

        private void OnEnable()
        {
            m_InteractableProperty = serializedObject.FindProperty("m_Interactable");
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
            m_ThrottleDurationProperty = serializedObject.FindProperty("m_ThrottleDuration");
            m_DebounceDurationProperty = serializedObject.FindProperty("m_DebounceDuration");
            m_AnimatorProperty = serializedObject.FindProperty("animator");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // [수정] 각 섹션을 명확하게 분리하여 그립니다.
            EnsureDefaultEvents();

            // --- 1. 기본 설정 (Interactable) ---
            EditorGUILayout.PropertyField(m_InteractableProperty);
            EditorGUILayout.Space(10);

            // --- 2. 고급 버튼 설정 ---
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Advanced Button Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_ThrottleDurationProperty);
            EditorGUILayout.PropertyField(m_DebounceDurationProperty);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // --- 3. 애니메이션 이벤트 ---
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Animation Events", EditorStyles.boldLabel);
            var eventsProp = m_AnimatorProperty.FindPropertyRelative("animationEvents");
            if (eventsProp != null && eventsProp.isArray)
            {
                DrawAnimationTabs(eventsProp);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // --- 4. 클릭 이벤트 ---
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Click Event", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_OnClickProperty);
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawAnimationTabs(SerializedProperty eventsProp)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            // --- 1. 이벤트 이름으로 탭 버튼들을 생성합니다. ---
            _tabs = new GUIContent[eventsProp.arraySize];
            for (int i = 0; i < eventsProp.arraySize; i++)
            {
                var eventName = eventsProp.GetArrayElementAtIndex(i).FindPropertyRelative("eventName").stringValue;
                _tabs[i] = new GUIContent(string.IsNullOrEmpty(eventName) ? "New Event" : eventName);
            }

            // --- 2. 탭과 함께 '+' '-' 버튼을 그립니다. ---
            EditorGUILayout.BeginHorizontal();
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs, EditorStyles.toolbarButton,
                GUILayout.ExpandWidth(true));

            // '+' 버튼으로 새로운 이벤트 추가
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), EditorStyles.toolbarButton,
                    GUILayout.Width(25)))
            {
                eventsProp.arraySize++;
                var newEvent = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);
                newEvent.FindPropertyRelative("eventName").stringValue = "New Custom Event";
                _selectedTab = eventsProp.arraySize - 1;
            }

            // '-' 버튼으로 선택된 이벤트 삭제 (기본 이벤트 제외)
            bool canRemove = _selectedTab >= 0 && _selectedTab < eventsProp.arraySize &&
                             !IsDefaultEvent(eventsProp.GetArrayElementAtIndex(_selectedTab));

            using (new EditorGUI.DisabledScope(!canRemove))
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), EditorStyles.toolbarButton,
                        GUILayout.Width(25)))
                {
                    var selectedEventProp = eventsProp.GetArrayElementAtIndex(_selectedTab);
                    var modulesProp = selectedEventProp.FindPropertyRelative("sequence.modules");
                    _moduleListsCache.Remove(modulesProp.propertyPath);
                    eventsProp.DeleteArrayElementAtIndex(_selectedTab);
                    if (_selectedTab >= eventsProp.arraySize) _selectedTab = eventsProp.arraySize - 1;
                }
            }

            EditorGUILayout.EndHorizontal();

            // --- 3. 선택된 탭에 해당하는 시퀀스를 그립니다. ---
            if (_selectedTab >= 0 && _selectedTab < eventsProp.arraySize)
            {
                var selectedEventProp = eventsProp.GetArrayElementAtIndex(_selectedTab);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);


                // 이벤트 이름 필드 (기본 이벤트는 수정 불가)
                using (new EditorGUI.DisabledScope(IsDefaultEvent(selectedEventProp)))
                {
                    EditorGUILayout.PropertyField(selectedEventProp.FindPropertyRelative("eventName"));
                }

                EditorGUILayout.PropertyField(selectedEventProp.FindPropertyRelative("sequence.mode"));

                var modulesProp = selectedEventProp.FindPropertyRelative("sequence.modules");
                if (!_moduleListsCache.TryGetValue(modulesProp.propertyPath, out var list))
                {
                    list = CreateModuleList(modulesProp);
                    _moduleListsCache[modulesProp.propertyPath] = list;
                }

                if (list != null) list.DoLayoutList();

                EditorGUILayout.PropertyField(selectedEventProp.FindPropertyRelative("onCompleted"));

                EditorGUILayout.Space(5);
                DrawControlBar(selectedEventProp);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawControlBar(SerializedProperty selectedEventProp)
        {
            // --- 컨트롤 바 그리기 ---
            EditorGUILayout.BeginHorizontal();
            var buttonStyle = EditorStyles.miniButton;

            using (new EditorGUI.DisabledScope(AnimatorPreviewer.IsPlaying))
            {
                if (GUILayout.Button("Play", buttonStyle))
                {
                    AnimatorPreviewer.PlayPreview(selectedEventProp.FindPropertyRelative("sequence"));
                }
            }

            // Revert 버튼 (Play 이후에 활성화되는 느낌을 주기 위해 재생 중 아닐 때만 활성화)
            using (new EditorGUI.DisabledScope(AnimatorPreviewer.IsPlaying))
            {
                if (GUILayout.Button("Revert", buttonStyle))
                {
                    AnimatorPreviewer.RevertPreview();
                }
            }
            
            using (new EditorGUI.DisabledScope(!AnimatorPreviewer.IsPlaying))
            {
                if (GUILayout.Button("Restart", buttonStyle))
                {
                    AnimatorPreviewer.RestartPreview();
                }

                if (GUILayout.Button("Stop", buttonStyle))
                {
                    AnimatorPreviewer.StopPreview();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void EnsureDefaultEvents()
        {
            var eventsProp = m_AnimatorProperty.FindPropertyRelative("animationEvents");

            if (!HasEvent(eventsProp, "OnHover"))
            {
                eventsProp.arraySize++;
                var hoverEvent = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);
                hoverEvent.FindPropertyRelative("eventName").stringValue = "OnHover";
            }

            if (!HasEvent(eventsProp, "OnClick"))
            {
                eventsProp.arraySize++;
                var clickEvent = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);
                clickEvent.FindPropertyRelative("eventName").stringValue = "OnClick";
            }
        }

        private bool IsDefaultEvent(SerializedProperty eventProp)
        {
            if (eventProp == null) return false;
            string eventName = eventProp.FindPropertyRelative("eventName").stringValue;
            return eventName == "OnHover" || eventName == "OnClick";
        }

        private bool HasEvent(SerializedProperty eventsProp, string eventName)
        {
            for (int i = 0; i < eventsProp.arraySize; i++)
            {
                if (eventsProp.GetArrayElementAtIndex(i).FindPropertyRelative("eventName").stringValue == eventName)
                    return true;
            }

            return false;
        }

        private ReorderableList CreateModuleList(SerializedProperty modulesProp)
        {
            if (modulesProp == null) return null;
            var list = new ReorderableList(serializedObject, modulesProp, true, true, true, true);

            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Animation Modules", EditorStyles.boldLabel);
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
                if (ModuleClipboard.ClipboardJson != null && ModuleClipboard.ClipboardType != null &&
                    typeof(UIAnimationModule).IsAssignableFrom(ModuleClipboard.ClipboardType))
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
                foreach (var type in TypeCache.GetTypesDerivedFrom<UIAnimationModule>())
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
                    menu.AddItem(new GUIContent("Remove"), false, () =>
                    {
                        modulesProp.DeleteArrayElementAtIndex(index);
                        // 변경 사항을 저장하고 인스펙터를 새로고침하라는 명령 추가
                        serializedObject.ApplyModifiedProperties();
                    });
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