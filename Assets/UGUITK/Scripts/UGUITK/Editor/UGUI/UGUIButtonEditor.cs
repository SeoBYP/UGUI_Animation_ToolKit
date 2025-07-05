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
    public class UGUIButtonEditor : UGUIEditor
    {
        // --- 프로퍼티 ---
        private SerializedProperty m_InteractableProperty;
        private SerializedProperty m_OnClickProperty;
        private SerializedProperty m_ThrottleDurationProperty;
        private SerializedProperty m_DebounceDurationProperty;
        private SerializedProperty m_AnimatorProperty;


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
            var eventsProp = m_AnimatorProperty.FindPropertyRelative("animationEvents");
            EnsureDefaultEvents(eventsProp,ButtonConstants.Events);

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
            if (eventsProp != null && eventsProp.isArray)
            {
                DrawAnimationTabs(eventsProp, ButtonConstants.Events);
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
    }
}
#endif