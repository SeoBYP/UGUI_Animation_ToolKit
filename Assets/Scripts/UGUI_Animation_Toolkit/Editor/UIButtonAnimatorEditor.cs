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
    struct MenuItem
    {
        public string Path;
        public Type Type;
        public int Order;
    }

    [CustomEditor(typeof(UIButtonAnimator))]
    public class UIButtonAnimatorEditor : UnityEditor.Editor
    {
        private ReorderableList _hoverList, _clickList;
        private SerializedProperty _hoverSequenceProp, _clickSequenceProp;

        // [추가] 현재 선택된 탭의 인덱스를 저장할 변수 (0: Hover, 1: Click)
        private int _selectedTab = 0;

        // [추가] 탭에 표시할 이름들
        private readonly GUIContent[] _tabs = { new GUIContent("Hover"), new GUIContent("Click") };

        private void OnEnable()
        {
            var so = serializedObject;
            _hoverSequenceProp = so.FindProperty("hoverSequence");
            _clickSequenceProp = so.FindProperty("clickSequence");

            _hoverList = CreateList(_hoverSequenceProp.FindPropertyRelative("modules"));
            _clickList = CreateList(_clickSequenceProp.FindPropertyRelative("modules"));
        }

        // [수정] OnInspectorGUI를 탭 기반 UI로 재구성합니다.
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);

            // 1. 탭을 그립니다. GUILayout.Toolbar는 선택된 탭의 인덱스를 반환합니다.
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabs, GUILayout.Height(25));

            EditorGUILayout.Space(10);

            // 2. 선택된 탭에 따라 다른 내용을 그립니다.
            switch (_selectedTab)
            {
                // "Hover" 탭이 선택된 경우
                case 0:
                    // DrawSequenceGroup을 호출하여 Hover 시퀀스 UI만 그립니다.
                    DrawSequenceGroup("Hover Animation Settings", _hoverList, _hoverSequenceProp);
                    break;

                // "Click" 탭이 선택된 경우
                case 1:
                    // DrawSequenceGroup을 호출하여 Click 시퀀스 UI만 그립니다.
                    DrawSequenceGroup("Click Animation Settings", _clickList, _clickSequenceProp);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        // DrawSequenceGroup 메서드에서 Foldout 관련 로직은 이제 필요 없으므로 제거하고 단순화합니다.
        private void DrawSequenceGroup(string title, ReorderableList list, SerializedProperty sequenceProp)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 헤더 라벨
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // AutoRevert, RevertDelay 필드 (필요하다면 여기에 그릴 수 있습니다)
            // EditorGUILayout.PropertyField(sequenceProp.FindPropertyRelative("AutoRevert"));
            // EditorGUILayout.PropertyField(sequenceProp.FindPropertyRelative("RevertDelay"));

            if (list != null)
            {
                list.DoLayoutList();
            }

            EditorGUILayout.EndVertical();
        }

        // CreateList 메서드는 이전 최종 버전과 동일합니다.
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
                var menuItems = new System.Collections.Generic.List<MenuItem>();

                // 1. 모든 모듈의 정보를 수집합니다.
                foreach (var type in TypeCache.GetTypesDerivedFrom<ButtonAnimationModule>())
                {
                    if (type.IsAbstract) continue;

                    var typeName = type.Name.Replace("Module", "");
                    var categoryAttribute = type.GetCustomAttribute<ModuleCategoryAttribute>();

                    var path = categoryAttribute?.Path != null ? $"{categoryAttribute.Path}/{typeName}" : typeName;
                    var order = categoryAttribute?.Order ?? int.MaxValue;

                    menuItems.Add(new MenuItem { Path = path, Type = type, Order = order });
                }

                // 2. 수집된 정보를 Order 값 기준으로 정렬합니다. Order가 같으면 경로(이름)순으로 정렬합니다.
                var sortedItems = menuItems.OrderBy(item => item.Order).ThenBy(item => item.Path);

                // 3. 정렬된 순서대로 GenericMenu에 아이템을 추가합니다.
                foreach (var item in sortedItems)
                {
                    menu.AddItem(new GUIContent(item.Path), false, () =>
                    {
                        modulesProp.arraySize++;
                        var element = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
                        // 클로저 문제를 피하기 위해 루프 변수인 item을 지역 변수로 복사합니다.
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