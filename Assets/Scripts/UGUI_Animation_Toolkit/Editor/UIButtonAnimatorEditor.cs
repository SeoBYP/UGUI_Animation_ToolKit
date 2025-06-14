#if UNITY_EDITOR
using System;
using System.Linq; // Last() 사용을 위해 추가
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UGUIAnimationToolkit.Editor
{
    [CustomEditor(typeof(UIButtonAnimator))]
    public class UIButtonAnimatorEditor : UnityEditor.Editor
    {
        // ... (OnEnable, OnInspectorGUI 등 다른 부분은 이전과 동일) ...
        private ReorderableList _enterList, _exitList, _downList, _upList;
        private SerializedProperty _enterSequenceProp, _exitSequenceProp, _downSequenceProp, _upSequenceProp;
        private SerializedProperty _rectTransformProp, _labelProp;

        private void OnEnable()
        {
            var so = serializedObject;

            _enterSequenceProp = so.FindProperty("enterSequence");
            _exitSequenceProp = so.FindProperty("exitSequence");
            _downSequenceProp = so.FindProperty("downSequence");
            _upSequenceProp = so.FindProperty("upSequence");

            _rectTransformProp = so.FindProperty("rectTransform");
            _labelProp = so.FindProperty("label");

            _enterList = CreateList("Enter Modules", _enterSequenceProp.FindPropertyRelative("modules"));
            _exitList = CreateList("Exit Modules", _exitSequenceProp.FindPropertyRelative("modules"));
            _downList = CreateList("Down Modules", _downSequenceProp.FindPropertyRelative("modules"));
            _upList = CreateList("Up Modules", _upSequenceProp.FindPropertyRelative("modules"));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Target Components", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(_rectTransformProp);
                EditorGUILayout.PropertyField(_labelProp);
            }

            EditorGUILayout.Space(10);

            DrawSequenceGroup("Enter Animation", _enterList, _enterSequenceProp);
            DrawSequenceGroup("Exit Animation", _exitList, _exitSequenceProp);
            DrawSequenceGroup("Down Animation", _downList, _downSequenceProp);
            DrawSequenceGroup("Up Animation", _upList, _upSequenceProp);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSequenceGroup(string title, ReorderableList list, SerializedProperty sequenceProp)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(sequenceProp.FindPropertyRelative("AutoRevert"));
            EditorGUILayout.PropertyField(sequenceProp.FindPropertyRelative("RevertDelay"));

            EditorGUILayout.Space(5);

            if (list != null)
                list.DoLayoutList();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private ReorderableList CreateList(string title, SerializedProperty modulesProp)
        {
            if (modulesProp == null) return null;

            // 드래그 기능은 비활성화된 상태를 유지합니다. (이전 요청사항)
            var list = new ReorderableList(serializedObject, modulesProp, true, true, true, true);

            list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, title, EditorStyles.boldLabel);

            // [수정 1] 높이 계산을 항상 '펼쳐진 상태' 기준으로 하도록 단순화합니다.
            list.elementHeightCallback = index =>
            {
                var element = modulesProp.GetArrayElementAtIndex(index);

                // if (element.isExpanded) 체크를 제거하여 항상 전체 높이를 계산합니다.
                float totalHeight = EditorGUIUtility.singleLineHeight; // 헤더 높이
                var prop = element.Copy();
                var endProp = prop.GetEndProperty();
                prop.NextVisible(true);

                while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, endProp))
                {
                    totalHeight += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
                }

                return totalHeight + 10; // 상하 여백
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
                        // element.isExpanded = true; // 더 이상 isExpanded를 사용하지 않으므로 이 줄은 필요 없습니다.
                        serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            };

            // [수정 2] 각 항목을 그릴 때 Foldout 대신 Label을 사용합니다.
            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = modulesProp.GetArrayElementAtIndex(index);

                rect.height -= EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.HelpBox(rect, string.Empty, MessageType.None);

                rect.y += 2;
                rect.height = EditorGUIUtility.singleLineHeight;
                rect.x += 10;
                rect.width -= 20;

                string typeName = element.managedReferenceFullTypename.Split('.').Last().Replace("Module", "");
                var descriptionProp = element.FindPropertyRelative("Description");
                string headerLabel = string.IsNullOrEmpty(descriptionProp.stringValue)
                    ? typeName
                    : $"{typeName}: {descriptionProp.stringValue}";

                // Foldout 대신 LabelField를 사용하여 화살표를 없앱니다. 헤더를 좀 더 강조하기 위해 bold 스타일을 적용합니다.
                EditorGUI.LabelField(rect, new GUIContent(headerLabel), EditorStyles.boldLabel);

                // if (element.isExpanded) 체크를 제거하여 항상 속성들을 그리도록 합니다.
                var prop = element.Copy();
                var endProp = prop.GetEndProperty();
                prop.NextVisible(true);

                EditorGUI.indentLevel++;

                while (prop.NextVisible(false) && !SerializedProperty.EqualContents(prop, endProp))
                {
                    rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    EditorGUI.PropertyField(rect, prop, true);
                }

                EditorGUI.indentLevel--;
            };

            return list;
        }
    }
}
#endif