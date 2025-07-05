#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UGUIAnimationToolkit.Core;
using UGUIAnimationToolkit.ProgressBar;
using UGUIAnimationToolkit.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UGUIAnimationToolkit.Editor
{
    /// <summary>
    /// UGUIAnimationToolkit의 모든 커스텀 에디터의 기반이 되는 추상 클래스입니다.
    /// 애니메이션 이벤트 탭, 모듈 리스트 등 공통 UI를 그리는 기능을 제공합니다.
    /// </summary>
    public abstract class UGUIEditor : UnityEditor.Editor
    {
        private int _selectedTab;
        private GUIContent[] _tabs;
        private readonly Dictionary<string, ReorderableList> _moduleListsCache = new();

        #region Public & Protected Methods

        /// <summary>
        /// 애니메이터 프로퍼티에 기본 이벤트들이 없으면 생성합니다.
        /// </summary>
        /// <param name="animatorProperty">"animator" 필드를 가리키는 SerializedProperty</param>
        /// <param name="defaultEvents">생성할 기본 이벤트 이름 배열</param>
        protected void EnsureDefaultEvents(SerializedProperty animatorProperty, string[] defaultEvents)
        {
            if (animatorProperty == null) return;
            var eventsProp = animatorProperty.FindPropertyRelative("animationEvents");
            if (eventsProp == null || !eventsProp.isArray) return;

            foreach (string eventName in defaultEvents)
            {
                if (!HasEvent(eventsProp, eventName))
                {
                    eventsProp.arraySize++;
                    var newElement = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);
                    newElement.FindPropertyRelative("eventName").stringValue = eventName;
                    serializedObject.ApplyModifiedProperties(); // 즉시 변경사항 적용
                }
            }
        }

        /// <summary>
        /// 애니메이션 이벤트와 모듈을 관리하는 전체 탭 UI를 그립니다.
        /// </summary>
        /// <param name="eventsProp">"animationEvents" 리스트를 가리키는 SerializedProperty</param>
        /// <param name="defaultEvents">삭제 불가능한 기본 이벤트 이름 배열</param>
        protected void DrawAnimationTabs(SerializedProperty eventsProp, string[] defaultEvents)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawTabsToolbar(eventsProp, defaultEvents);
            DrawSelectedEventDetails(eventsProp, defaultEvents);
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Private: Drawing UI

        /// <summary>
        /// 이벤트 탭과 추가/삭제 버튼이 있는 툴바를 그립니다.
        /// </summary>
        private void DrawTabsToolbar(SerializedProperty eventsProp, string[] defaultEvents)
        {
            _tabs = new GUIContent[eventsProp.arraySize];
            for (int i = 0; i < eventsProp.arraySize; i++)
            {
                var eventName = eventsProp.GetArrayElementAtIndex(i).FindPropertyRelative("eventName").stringValue;
                _tabs[i] = new GUIContent(string.IsNullOrEmpty(eventName) ? "New Event" : eventName);
            }

            EditorGUILayout.BeginHorizontal();
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs, EditorStyles.toolbarButton,
                GUILayout.ExpandWidth(true));

            // '+' 버튼 (새 이벤트 추가)
            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus"), EditorStyles.toolbarButton,
                    GUILayout.Width(25)))
            {
                eventsProp.arraySize++;
                var newEvent = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);
                newEvent.FindPropertyRelative("eventName").stringValue = "New Custom Event";
                _selectedTab = eventsProp.arraySize - 1;
            }

            // '-' 버튼 (선택된 이벤트 삭제)
            bool canRemove = _selectedTab >= 0 && _selectedTab < eventsProp.arraySize &&
                             !IsDefaultEvent(eventsProp.GetArrayElementAtIndex(_selectedTab), defaultEvents);
            using (new EditorGUI.DisabledScope(!canRemove))
            {
                if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), EditorStyles.toolbarButton,
                        GUILayout.Width(25)))
                {
                    var modulesProp = eventsProp.GetArrayElementAtIndex(_selectedTab).FindPropertyRelative("modules");
                    if (modulesProp != null) _moduleListsCache.Remove(modulesProp.propertyPath);

                    eventsProp.DeleteArrayElementAtIndex(_selectedTab);
                    if (_selectedTab >= eventsProp.arraySize) _selectedTab = eventsProp.arraySize - 1;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 선택된 이벤트 탭의 상세 내용을 그립니다.
        /// </summary>
        private void DrawSelectedEventDetails(SerializedProperty eventsProp, string[] defaultEvents)
        {
            if (_selectedTab < 0 || _selectedTab >= eventsProp.arraySize) return;

            var selectedEventProp = eventsProp.GetArrayElementAtIndex(_selectedTab);
            EditorGUILayout.BeginVertical();

            // 이벤트 이름 필드 (기본 이벤트는 수정 불가)
            using (new EditorGUI.DisabledScope(IsDefaultEvent(selectedEventProp, defaultEvents)))
            {
                EditorGUILayout.PropertyField(selectedEventProp.FindPropertyRelative("eventName"));
            }

            // sequence 객체 내부의 mode 프로퍼티를 그리도록 경로 수정
            EditorGUILayout.PropertyField(selectedEventProp.FindPropertyRelative("sequence.mode"));

            // sequence 객체 내부의 modules 프로퍼티를 찾도록 경로 수정
            var modulesProp = selectedEventProp.FindPropertyRelative("sequence.modules");

            // modulesProp이 null일 경우를 대비한 방어 코드
            if (modulesProp != null)
            {
                if (!_moduleListsCache.TryGetValue(modulesProp.propertyPath, out var list))
                {
                    list = CreateModuleList(modulesProp);
                    _moduleListsCache[modulesProp.propertyPath] = list;
                }

                list?.DoLayoutList();
            }

            EditorGUILayout.PropertyField(selectedEventProp.FindPropertyRelative("onCompleted"));
            EditorGUILayout.Space(5);
            DrawControlBar(selectedEventProp);

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 애니메이션 미리보기 컨트롤 바를 그립니다.
        /// </summary>
        private void DrawControlBar(SerializedProperty selectedEventProp)
        {
            EditorGUILayout.BeginHorizontal();
            var buttonStyle = EditorStyles.miniButton;

            using (new EditorGUI.DisabledScope(AnimatorPreviewer.IsPlaying))
            {
                if (GUILayout.Button(UGUIAnimationConstants.Play, buttonStyle))
                    AnimatorPreviewer.PlayPreview(selectedEventProp);
                if (GUILayout.Button(UGUIAnimationConstants.Revert, buttonStyle)) AnimatorPreviewer.RevertPreview();
            }

            using (new EditorGUI.DisabledScope(!AnimatorPreviewer.IsPlaying))
            {
                if (GUILayout.Button(UGUIAnimationConstants.Restart, buttonStyle)) AnimatorPreviewer.RestartPreview();
                if (GUILayout.Button(UGUIAnimationConstants.Stop, buttonStyle)) AnimatorPreviewer.StopPreview();
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Private: ReorderableList

        /// <summary>
        /// 애니메이션 모듈을 위한 ReorderableList를 생성합니다.
        /// </summary>
        private ReorderableList CreateModuleList(SerializedProperty modulesProp)
        {
            if (modulesProp == null) return null;

            var list = new ReorderableList(serializedObject, modulesProp, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Animation Modules", EditorStyles.boldLabel),
                elementHeightCallback = index => GetModuleElementHeight(modulesProp, index),
                onAddDropdownCallback = (rect, l) => BuildModuleAddMenu(modulesProp).ShowAsContext(),
                drawElementCallback = (rect, index, active, focused) =>
                    DrawModuleElementInternal(rect, index, modulesProp)
            };
            return list;
        }

        private void DrawModuleElementInternal(Rect rect, int index, SerializedProperty modulesProp)
        {
// modulesProp과 index를 사용해 현재 요소를 가져옴
            var element = modulesProp.GetArrayElementAtIndex(index);
            rect.y += 2;

            if (element.managedReferenceValue == null)
            {
                EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight * 2 - 4),
                    "Module is null or missing.", MessageType.Warning);
                return;
            }

            var headerRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.DrawRect(headerRect, new Color(0.18f, 0.18f, 0.18f));

            var enableProp = element.FindPropertyRelative("Enable");
            var enableRect = new Rect(headerRect.x + 5, headerRect.y, 15, headerRect.height);
            if (enableProp != null) enableProp.boolValue = EditorGUI.Toggle(enableRect, enableProp.boolValue);

            string typeName = element.managedReferenceFullTypename.Split('.').Last().Replace("Module", "");
            var labelRect = new Rect(enableRect.xMax, headerRect.y, headerRect.width - 60, headerRect.height);
            if (GUI.Button(labelRect, typeName, EditorStyles.boldLabel))
            {
                element.isExpanded = !element.isExpanded;
            }

            var menuRect = new Rect(headerRect.xMax - 25, headerRect.y, 25, headerRect.height);
            if (EditorGUI.DropdownButton(menuRect, EditorGUIUtility.IconContent("_Menu"), FocusType.Passive,
                    EditorStyles.label))
            {
                // [수정] 잘못된 GetArray() 호출 대신, 파라미터로 받은 modulesProp을 그대로 사용
                BuildElementActionMenu(element, modulesProp, index).ShowAsContext();
            }

            if (element.isExpanded)
            {
                var detailsBgRect = new Rect(rect.x, headerRect.yMax, rect.width, rect.height - headerRect.height);
                EditorGUI.HelpBox(detailsBgRect, "", MessageType.None);

                var contentRect = new Rect(detailsBgRect.x + 10, detailsBgRect.y + 5, detailsBgRect.width - 20, 0);

                var prop = element.Copy();
                var endProp = prop.GetEndProperty();
                prop.NextVisible(true);

                while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, endProp))
                {
                    contentRect.height = EditorGUI.GetPropertyHeight(prop, true);
                    EditorGUI.PropertyField(contentRect, prop, true);
                    contentRect.y += contentRect.height + EditorGUIUtility.standardVerticalSpacing;
                }
            }
        }

        /// <summary>
        /// 모듈 추가(+) 버튼을 눌렀을 때 나타나는 GenericMenu를 생성합니다.
        /// </summary>
        private GenericMenu BuildModuleAddMenu(SerializedProperty modulesProp)
        {
            var menu = new GenericMenu();
            var componentType = target.GetType();

            // 1. 클립보드에 복사된 모듈이 있으면 "Paste As New" 메뉴 추가 (필터링 적용)
            if (ModuleClipboard.ClipboardJson != null && ModuleClipboard.ClipboardType != null &&
                typeof(UIAnimationModule).IsAssignableFrom(ModuleClipboard.ClipboardType))
            {
                if (IsModuleTypeCompatible(ModuleClipboard.ClipboardType, componentType))
                {
                    var pasteName = ModuleClipboard.ClipboardType.Name.Replace("Module", "");
                    menu.AddItem(new GUIContent($"Paste As New ({pasteName})"), false, () =>
                    {
                        var newModule = Activator.CreateInstance(ModuleClipboard.ClipboardType);
                        JsonUtility.FromJsonOverwrite(ModuleClipboard.ClipboardJson, newModule);
                        AddModule(modulesProp, newModule);
                    });
                    menu.AddSeparator("");
                }
            }

            // 2. 추가 가능한 모든 모듈을 메뉴에 추가 (필터링 적용)
            var menuItems = TypeCache.GetTypesDerivedFrom<UIAnimationModule>()
                .Where(type => !type.IsAbstract && IsModuleTypeCompatible(type, componentType))
                .Select(type =>
                {
                    var categoryAttribute = type.GetCustomAttribute<ModuleCategoryAttribute>();
                    var typeName = type.Name.Replace("Module", "");
                    var path = categoryAttribute?.Path != null ? $"{categoryAttribute.Path}/{typeName}" : typeName;
                    return new MenuItem { Path = path, Type = type, Order = categoryAttribute?.Order ?? int.MaxValue };
                })
                .OrderBy(item => item.Order)
                .ThenBy(item => item.Path);

            foreach (var item in menuItems)
            {
                menu.AddItem(new GUIContent(item.Path), false, () =>
                {
                    var newModule = Activator.CreateInstance(item.Type);
                    AddModule(modulesProp, newModule);
                });
            }

            return menu;
        }

        /// <summary>
        /// 모듈 요소의 높이를 계산합니다.
        /// </summary>
        private float GetModuleElementHeight(SerializedProperty modulesProp, int index)
        {
            if (index < 0 || index >= modulesProp.arraySize) return EditorGUIUtility.singleLineHeight;
            
            var element = modulesProp.GetArrayElementAtIndex(index);

            if (element == null || element.managedReferenceValue == null)
            {
                return EditorGUIUtility.singleLineHeight * 2;
            }

            if (!element.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight + 4;
            }

            float totalHeight = EditorGUIUtility.singleLineHeight + 4;
            
            var prop = element.Copy();
            var endProp = prop.GetEndProperty();
            prop.NextVisible(true);
            while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, endProp))
            {
                totalHeight += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
            }
            
            totalHeight += 10;
            return totalHeight;
        }

        /// <summary>
        /// 모듈 요소의 액션 메뉴 (...)를 생성합니다.
        /// </summary>
        private GenericMenu BuildElementActionMenu(SerializedProperty element, SerializedProperty modulesProp,
            int index)
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
                serializedObject.ApplyModifiedProperties();
            });
            return menu;
        }

        /// <summary>
        /// 모듈을 리스트에 추가하고 Inspector를 업데이트합니다.
        /// </summary>
        private void AddModule(SerializedProperty modulesProp, object moduleInstance)
        {
            modulesProp.arraySize++;
            var element = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
            element.managedReferenceValue = moduleInstance;
            element.isExpanded = true;
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private: Helpers

        public bool IsDefaultEvent(SerializedProperty animationEventProperty, string[] defaultEvents)
        {
            if (animationEventProperty == null) return false;
            string eventName = animationEventProperty.FindPropertyRelative("eventName").stringValue;
            return defaultEvents.Any(x => x == eventName);
        }

        private bool HasEvent(SerializedProperty eventsProp, string eventName)
        {
            for (int i = 0; i < eventsProp.arraySize; i++)
            {
                var element = eventsProp.GetArrayElementAtIndex(i);
                var nameProp = element.FindPropertyRelative("eventName");
                if (nameProp != null && nameProp.stringValue == eventName)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// 특정 모듈 타입이 현재 컴포넌트 타입과 호환되는지 확인합니다.
        /// </summary>
        private bool IsModuleTypeCompatible(Type moduleType, Type componentType)
        {
            bool isTextModule = typeof(TextAnimationModule).IsAssignableFrom(moduleType);
            bool isProgressBarModule = typeof(ProgressBarAnimationModule).IsAssignableFrom(moduleType);

            if (isTextModule && !typeof(UGUIText).IsAssignableFrom(componentType)) return false;
            if (isProgressBarModule && !typeof(UGUIProgressBar).IsAssignableFrom(componentType)) return false;

            return true;
        }

        #endregion
    }
}
#endif