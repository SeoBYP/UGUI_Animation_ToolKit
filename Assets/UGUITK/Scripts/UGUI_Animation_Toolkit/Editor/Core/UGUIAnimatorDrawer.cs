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

            // ReorderableList를 그립니다.
            // 전체를 Box로 감싸 통일성을 줍니다.
            EditorGUI.BeginProperty(position, label, eventsProp);
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
                EditorGUI.PropertyField(rect, element, GUIContent.none);
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