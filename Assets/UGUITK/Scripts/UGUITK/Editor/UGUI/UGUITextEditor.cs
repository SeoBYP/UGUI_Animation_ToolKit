#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UGUIAnimationToolkit.Core;
using UGUIAnimationToolkit.Editor;
using UGUIAnimationToolkit.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UGUIAnimationToolkit.Editor // 네임스페이스를 Text.Editor로 명확히 하는 것을 추천합니다.
{
    [CustomEditor(typeof(UGUIText))]
    public class UGUITextEditor : UGUIEditor
    {
        private SerializedProperty m_TextComponentProperty;
        private SerializedProperty m_TextProperty;
        private SerializedProperty m_OnTextChangedProperty;
        private SerializedProperty m_AnimatorProperty;

        private void OnEnable()
        {
            m_TextComponentProperty = serializedObject.FindProperty("_textMeshProUGUI");
            m_TextProperty = serializedObject.FindProperty("_text");
            m_OnTextChangedProperty = serializedObject.FindProperty("_onTextChanged");
            m_AnimatorProperty = serializedObject.FindProperty("animator");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_TextComponentProperty);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Text Content", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_TextProperty);

            // TextAnimator의 기본 이벤트("OnTextChange")가 없으면 생성해줍니다.
            EnsureDefaultEvents(m_AnimatorProperty, TextConstants.Events);

            var eventsProp = m_AnimatorProperty.FindPropertyRelative("animationEvents");
            if (eventsProp != null && eventsProp.isArray)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Animation Events", EditorStyles.boldLabel);
                // UGUIEditor의 탭 그리기 함수를 사용하여 애니메이션 UI를 그립니다.
                DrawAnimationTabs(eventsProp, TextConstants.Events);
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Text Changed Event (OnTextChanged)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_OnTextChangedProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif