using UGUIAnimationToolkit.Core;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace UGUIAnimationToolkit.Eidtor.Core
{
    [CustomPropertyDrawer(typeof(UIAnimationEvent))]
    public class UIAnimationEventDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var eventNameProp = property.FindPropertyRelative("eventName");

            // 전체를 box로 감싸줍니다.
            EditorGUI.BeginProperty(position, label, eventNameProp);
            var boxRect = new Rect(position.x, position.y, position.width, GetPropertyHeight(property, label) - 4);
            GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

            // Foldout 헤더
            var headerRect = new Rect(position.x, position.y + 2, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded,
                string.IsNullOrEmpty(eventNameProp.stringValue) ? "New Event" : eventNameProp.stringValue, true,
                EditorStyles.boldLabel);

            if (property.isExpanded)
            {
                var contentPosition = new Rect(position.x + 15, position.y + EditorGUIUtility.singleLineHeight + 4, position.width - 20, position.height);
                var currentY = contentPosition.y;
                // Event Name
                var eventNameRect = new Rect(contentPosition.x, currentY, contentPosition.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(eventNameRect, eventNameProp, new GUIContent("Event Name"));
                currentY += EditorGUI.GetPropertyHeight(eventNameProp) + 4;
                
                // Sequence
                var sequenceProp = property.FindPropertyRelative("sequence");
                var sequenceRect = new Rect(contentPosition.x, currentY, contentPosition.width,EditorGUI.GetPropertyHeight(sequenceProp,true));
                EditorGUI.PropertyField(eventNameRect, sequenceProp, new GUIContent("Sequence"),true);
                currentY += sequenceRect.height + 4;
                
                // On Completed
                var onCompletedProp = property.FindPropertyRelative("onCompleted");
                var onCompletedRect = new Rect(contentPosition.x, currentY, contentPosition.width,EditorGUI.GetPropertyHeight(onCompletedProp,true));
                EditorGUI.PropertyField(onCompletedRect, onCompletedProp, new GUIContent("On Completed"));
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
            
            float height = EditorGUIUtility.singleLineHeight + 8;
            height += EditorGUI.GetPropertyHeight(eventNameProp, true) + 4;
            height += EditorGUI.GetPropertyHeight(sequenceProp, true) + 4;
            height += EditorGUI.GetPropertyHeight(onCompletedProp, true) + 4;

            return height;
        }
    }
}
#endif