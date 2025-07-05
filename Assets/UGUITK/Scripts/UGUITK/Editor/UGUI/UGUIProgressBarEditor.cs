using System;
using System.Linq;
using System.Reflection;
using UGUIAnimationToolkit;
using UGUIAnimationToolkit.Core;
using UGUIAnimationToolkit.Editor;
using UGUIAnimationToolkit.ProgressBar;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using MenuItem = UGUIAnimationToolkit.Editor.MenuItem;

namespace UGUIAnimationToolkit.ProgressBar.Editor
{
    [CustomEditor(typeof(UGUIProgressBar))]
    [CanEditMultipleObjects]
    public class UGUIProgressBarEditor : UGUIEditor
    {
        // --- 프로퍼티 ---
        private SerializedProperty m_FillImageProperty;
        private SerializedProperty m_MinValueProperty;
        private SerializedProperty m_MaxValueProperty;
        private SerializedProperty m_ValueProperty;
        private SerializedProperty m_DirectionProperty;
        private SerializedProperty m_OnValueChangedProperty;

        // --- 애니메이션 시퀀스 프로퍼티 ---
        private SerializedProperty m_AnimatorProperty;

        private void OnEnable()
        {
            // 프로그레스 바의 모든 직렬화된 필드를 찾아옵니다.
            m_FillImageProperty = serializedObject.FindProperty("_fillImage");
            m_MinValueProperty = serializedObject.FindProperty("_minValue");
            m_MaxValueProperty = serializedObject.FindProperty("_maxValue");
            m_ValueProperty = serializedObject.FindProperty("_value");
            m_DirectionProperty = serializedObject.FindProperty("_direction");
            m_OnValueChangedProperty = serializedObject.FindProperty("_OnValueChanged");
            m_AnimatorProperty = serializedObject.FindProperty("animator");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var eventsProp = m_AnimatorProperty.FindPropertyRelative("animationEvents");
            EnsureDefaultEvents(eventsProp, ProgressBarConstants.Events);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("UI Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_FillImageProperty, new GUIContent("Fill Image"));
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Value Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_MinValueProperty);
            EditorGUILayout.PropertyField(m_MaxValueProperty);

            // 값을 직접 수정하는 슬라이더 UI 추가
            float min = m_MinValueProperty.floatValue;
            float max = m_MaxValueProperty.floatValue;
            float val = m_ValueProperty.floatValue;
            EditorGUI.BeginChangeCheck();
            val = EditorGUILayout.Slider("Value", val, min, max);
            if (EditorGUI.EndChangeCheck())
            {
                m_ValueProperty.floatValue = val;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(m_DirectionProperty);

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Animation Events", EditorStyles.boldLabel);
            if (eventsProp != null && eventsProp.isArray)
            {
                DrawAnimationTabs(eventsProp, ProgressBarConstants.Events);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Value Changed Event", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_OnValueChangedProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}