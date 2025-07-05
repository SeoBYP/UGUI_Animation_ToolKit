#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UGUIAnimationToolkit.Core;

namespace UGUIAnimationToolkit.Editor
{
    [CustomPropertyDrawer(typeof(UIAnimationEvent))]
    public class UIAnimationEventDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var eventNameProp = property.FindPropertyRelative("eventName");

            EditorGUI.BeginProperty(position, label, property);

            var boxRect = new Rect(position.x, position.y + 2, position.width, GetPropertyHeight(property, label) - 4);
            GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

            var headerRect = new Rect(position.x, position.y + 2, position.width, EditorGUIUtility.singleLineHeight);

            // Foldout을 그리는 영역을 약간 줄여서 버튼 공간을 확보합니다.
            var foldoutRect = new Rect(headerRect.x, headerRect.y, headerRect.width - 24, headerRect.height);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded,
                string.IsNullOrEmpty(eventNameProp.stringValue) ? "New Event" : eventNameProp.stringValue, true,
                EditorStyles.boldLabel);
            
            if (property.isExpanded)
            {
                var contentPosition = new Rect(position.x + 15, position.y + EditorGUIUtility.singleLineHeight + 4,
                    position.width - 20, position.height);
                var currentY = contentPosition.y;

                // Event Name 필드 (이름이 OnHover, OnClick이면 수정 불가)
                bool isDefaultEvent = eventNameProp.stringValue == "OnHover" || eventNameProp.stringValue == "OnClick";
                using (new EditorGUI.DisabledScope(isDefaultEvent))
                {
                    var eventNameRect = new Rect(contentPosition.x, currentY, contentPosition.width,
                        EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(eventNameRect, eventNameProp, new GUIContent("Event Name"));
                }

                currentY += EditorGUI.GetPropertyHeight(eventNameProp) + 4;

                // Sequence 필드
                var sequenceProp = property.FindPropertyRelative("sequence");
                var sequenceRect = new Rect(contentPosition.x, currentY, contentPosition.width,
                    EditorGUI.GetPropertyHeight(sequenceProp, true));
                EditorGUI.PropertyField(sequenceRect, sequenceProp, new GUIContent("Sequence"), true);
                currentY += sequenceRect.height + 4;

                // On Completed 이벤트 필드
                // var onCompletedProp = property.FindPropertyRelative("onCompleted");
                // var onCompletedRect = new Rect(contentPosition.x, currentY, contentPosition.width, EditorGUI.GetPropertyHeight(onCompletedProp, true));
                // EditorGUI.PropertyField(onCompletedRect, onCompletedProp, new GUIContent("On Completed"), true);
                currentY += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("onCompleted"), true) + 8;
                var controlBarRect = new Rect(contentPosition.x, currentY, contentPosition.width,
                    EditorGUIUtility.singleLineHeight);
            }

            EditorGUI.EndProperty();
        }

        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
            {
                return EditorGUIUtility.singleLineHeight + 4;
            }

            var eventNameProp = property.FindPropertyRelative("eventName");
            var sequenceProp = property.FindPropertyRelative("sequence");
            var onCompletedProp = property.FindPropertyRelative("onCompleted");

            float height = EditorGUIUtility.singleLineHeight + 8; // Header
            height += EditorGUI.GetPropertyHeight(eventNameProp, true) + 4;
            height += EditorGUI.GetPropertyHeight(sequenceProp, true) + 4;
            height += EditorGUI.GetPropertyHeight(onCompletedProp, true) + 4;

            return height;
        }
    }
}
#endif