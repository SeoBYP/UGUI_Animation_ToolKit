#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UGUIAnimationToolkit.Editor
{
    [CustomEditor(typeof(UIButtonAnimator))]
    public class UIButtonAnimatorEditor : UnityEditor.Editor
    {
        private ReorderableList _enterList, _exitList, _downList, _upList;
        private SerializedProperty _enterSequenceProp, _exitSequenceProp, _downSequenceProp, _upSequenceProp;
        private bool _enterFoldout = true, _exitFoldout = true, _downFoldout = true, _upFoldout = true;

        private void OnEnable()
        {
            var so = serializedObject;
            _enterSequenceProp = so.FindProperty("enterSequence");
            _exitSequenceProp = so.FindProperty("exitSequence");
            _downSequenceProp = so.FindProperty("downSequence");
            _upSequenceProp = so.FindProperty("upSequence");

            _enterList = CreateList(_enterSequenceProp.FindPropertyRelative("modules"));
            _exitList = CreateList(_exitSequenceProp.FindPropertyRelative("modules"));
            _downList = CreateList(_downSequenceProp.FindPropertyRelative("modules"));
            _upList = CreateList(_upSequenceProp.FindPropertyRelative("modules"));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // [수정] DrawComponentsSection() 호출을 완전히 삭제했습니다.

            _enterFoldout = DrawSequenceGroup("Enter Animation", _enterList, _enterSequenceProp, _enterFoldout);
            _exitFoldout = DrawSequenceGroup("Exit Animation", _exitList, _exitSequenceProp, _exitFoldout);
            _downFoldout = DrawSequenceGroup("Down Animation", _downList, _downSequenceProp, _downFoldout);
            _upFoldout = DrawSequenceGroup("Up Animation", _upList, _upSequenceProp, _upFoldout);

            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawSequenceGroup(string title, ReorderableList list, SerializedProperty sequenceProp,
            bool foldoutState)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            Rect headerRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight + 2);
            EditorGUI.DrawRect(headerRect, new Color(0.22f, 0.22f, 0.22f));

            var titleRect = new Rect(headerRect.x + 5, headerRect.y + 1, headerRect.width - 10,
                EditorGUIUtility.singleLineHeight);
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft };
            if (GUI.Button(titleRect, title, buttonStyle))
            {
                foldoutState = !foldoutState;
            }

            if (foldoutState)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(sequenceProp.FindPropertyRelative("AutoRevert"));
                EditorGUILayout.PropertyField(sequenceProp.FindPropertyRelative("RevertDelay"));
                EditorGUILayout.Space(5);
                if (list != null) list.DoLayoutList();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
            return foldoutState;
        }

// ▼▼▼▼▼ UI 깨짐 문제가 해결된 최종 CreateList 메서드 ▼▼▼▼▼
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

                // EditorGUI.GetPropertyHeight가 모든 자식 속성과 Header, Space를 포함하여
                // 정확한 전체 높이를 계산해주므로 이 부분은 올바릅니다.
                return EditorGUI.GetPropertyHeight(element, true) + 10;
            };

            list.onAddDropdownCallback = (rect, l) =>
            {
                var menu = new GenericMenu();
                foreach (var type in TypeCache.GetTypesDerivedFrom<ButtonAnimationModule>())
                {
                    if (type.IsAbstract) continue;
                    menu.AddItem(new GUIContent(type.Name.Replace("Module", "")), false, () =>
                    {
                        modulesProp.arraySize++;
                        var element = modulesProp.GetArrayElementAtIndex(modulesProp.arraySize - 1);
                        element.managedReferenceValue = Activator.CreateInstance(type);
                        element.isExpanded = true; // 새로 추가된 모듈은 펼쳐진 상태로 시작
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

                // -- 헤더 바 그리기 (이전과 동일) --
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

                // -- 모듈 내용 그리기 --
                if (element.isExpanded)
                {
                    // [수정] element 전체를 한 번에 그리는 대신, 자식 속성들을 순회하며 하나씩 그립니다.
                    // 이것이 UI가 깨지지 않는 가장 안정적인 방법입니다.
                    var contentRect = new Rect(rect.x, headerRect.yMax + 4, rect.width, 0);

                    var prop = element.Copy();
                    var endProp = prop.GetEndProperty();
                    prop.NextVisible(true); // 첫 번째 자식으로 이동 (Description, Enable 등)

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