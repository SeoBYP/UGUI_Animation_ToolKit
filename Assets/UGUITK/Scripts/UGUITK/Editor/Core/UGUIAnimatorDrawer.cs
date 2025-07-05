#if UNITY_EDITOR
using System.Collections.Generic;
using UGUIAnimationToolkit.Core;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UGUIAnimationToolkit.Eidtor.Core
{
    [CustomPropertyDrawer(typeof(UGUIAnimator), true)]
    public class UGUIAnimatorDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, ReorderableList> _reorderableLists =
            new Dictionary<string, ReorderableList>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var eventsProp = property.FindPropertyRelative("animationEvents");
            if (eventsProp == null)
            {
                EditorGUI.LabelField(position, "Could not find 'animationEvents' property.");
                return;
            }

            if (!_reorderableLists.TryGetValue(property.propertyPath, out var list))
            {
                list = CreateEventList(eventsProp);
                _reorderableLists.Add(property.propertyPath, list);
            }

            // ReorderableList를 그립니다..
            EditorGUI.BeginProperty(position, label, eventsProp);
            // [수정] 헤더 라벨을 제대로 표시하기 위해 label을 전달합니다.
            list.DoList(position);
            EditorGUI.EndProperty();
        }

        private ReorderableList CreateEventList(SerializedProperty eventsProp)
        {
            var list = new ReorderableList(eventsProp.serializedObject, eventsProp, true, true, true, true);
            list.drawHeaderCallback = (Rect rect) =>
                EditorGUI.LabelField(rect, "Animation Events", EditorStyles.boldLabel);

            // 각 이벤트 요소는 UIAnimationEventDrawer가 그리므로, 높이 계산을 맡깁니다.
            list.elementHeightCallback = index =>
                EditorGUI.GetPropertyHeight(eventsProp.GetArrayElementAtIndex(index), true);
            // 각 요소는 UIAnimationEventDrawer가 알아서 그려줍니다.
            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.y += 2;
                var element = eventsProp.GetArrayElementAtIndex(index);
                // [수정] GUIContent.none 대신 element.displayName을 사용합니다.
                EditorGUI.PropertyField(rect, element, new GUIContent(element.displayName), true);
            };
            return list;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var eventsProp = property.FindPropertyRelative("animationEvents");
            if (eventsProp == null)
                return EditorGUIUtility.singleLineHeight;

            if (!_reorderableLists.TryGetValue(property.propertyPath, out var list))
            {
                list = CreateEventList(eventsProp);
                _reorderableLists.Add(property.propertyPath, list);
            }

            return list.GetHeight();
        }
    }
}
#endif