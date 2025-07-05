#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UGUIAnimationToolkit.Core;
using UnityEngine.UI;

namespace UGUIAnimationToolkit.Editor
{
    public static class AnimatorPreviewer
    {
       private static readonly Dictionary<Object, Dictionary<string, object>> _initialStates = new Dictionary<Object, Dictionary<string, object>>();
        private static UIAnimationContext _previewContext = new UIAnimationContext(null);
        
        public static bool IsPlaying { get; private set; }
        private static SerializedProperty _currentSequenceProp;
        private static AnimationSequence _currentTempSequence; // [추가] Play와 Revert가 공유할 시퀀스

        /// <summary>
        /// 현재 진행 중인 모든 프리뷰 애니메이션을 중지하고 상태를 복원합니다.
        /// </summary>
        public static void StopPreview()
        {
            if (!IsPlaying && _initialStates.Count == 0) return;
            
            _previewContext.MotionHandle.Cancel();
            RestoreInitialStates();
            
            IsPlaying = false;
            _currentSequenceProp = null;
            _currentTempSequence = null;
        }

        /// <summary>
        /// 지정된 시퀀스의 'Play' 프리뷰를 재생합니다.
        /// </summary>
        public static async void PlayPreview(SerializedProperty sequenceProp)
        {
            StopPreview();
            
            _currentSequenceProp = sequenceProp;
            var owner = sequenceProp.serializedObject.targetObject as Component;
            _previewContext = new UIAnimationContext(owner);
            
            CaptureInitialStates(sequenceProp);
            
            _currentTempSequence = new AnimationSequence();
            var modulesProp = sequenceProp.FindPropertyRelative("modules");
            for(int i = 0; i < modulesProp.arraySize; i++)
            {
                if (modulesProp.GetArrayElementAtIndex(i).managedReferenceValue is UIAnimationModule module)
                {
                    _currentTempSequence.modules.Add(module);
                }
            }

            IsPlaying = true;
            
            try
            {
                await _currentTempSequence.PlayAsync(_previewContext);
            }
            finally
            {
                // 애니메이션이 끝나도 StopPreview를 호출하지 않아 상태를 유지합니다.
                IsPlaying = false;
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        
        /// <summary>
        /// 현재 시퀀스의 'Revert' 프리뷰를 재생합니다.
        /// </summary>
        public static async void RevertPreview()
        {
            // Play가 먼저 실행되어 상태가 저장된 경우에만 동작합니다.
            if (_currentTempSequence == null)
            {
                Debug.LogWarning("RevertPreview can only be used after PlayPreview has been run.");
                return;
            }
            
            _previewContext.MotionHandle.Cancel();
            IsPlaying = true;

            try
            {
                await _currentTempSequence.RevertAsync(_previewContext);
            }
            finally
            {
                IsPlaying = false;
                EditorApplication.RepaintHierarchyWindow();
            }
        }
        
        public static void RestartPreview()
        {
            if (_currentSequenceProp != null)
            {
                PlayPreview(_currentSequenceProp);
            }
        }
        
        // --- 상태 저장 및 복원 로직 ---
        private static void CaptureInitialStates(SerializedProperty sequenceProp)
        {
            _initialStates.Clear();
            var modulesProp = sequenceProp.FindPropertyRelative("modules");
            if(modulesProp.arraySize == 0) 
                return;
            for (int i = 0; i < modulesProp.arraySize; i++)
            {
                var module = modulesProp.GetArrayElementAtIndex(i).managedReferenceValue;
                if(module == null) continue;
                
                var fields = module.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var fieldValue = field.GetValue(module);

                    if (fieldValue is Component targetComponent)
                    {
                        var targetObject = targetComponent.transform;
                        if (!_initialStates.ContainsKey(targetObject))
                        {
                            _initialStates[targetObject] = new Dictionary<string, object>
                            {
                                { "localPosition", targetObject.localPosition },
                                { "localRotation", targetObject.localRotation },
                                { "localScale", targetObject.localScale }
                            };
                        }
                    }
                    else if (fieldValue is Graphic targetGraphic)
                    {
                        if (!_initialStates.ContainsKey(targetGraphic))
                        {
                            _initialStates[targetGraphic] = new Dictionary<string, object> { { "color", targetGraphic.color } };
                        }
                    }
                    else if (fieldValue is CanvasGroup targetCanvasGroup)
                    {
                         if (!_initialStates.ContainsKey(targetCanvasGroup))
                        {
                            _initialStates[targetCanvasGroup] = new Dictionary<string, object> { { "alpha", targetCanvasGroup.alpha } };
                        }
                    }
                }
            }
        }

        private static void RestoreInitialStates()
        {
            foreach (var stateEntry in _initialStates)
            {
                var target = stateEntry.Key;
                var values = stateEntry.Value;

                if (target is Transform transform)
                {
                    transform.localPosition = (Vector3)values["localPosition"];
                    transform.localRotation = (Quaternion)values["localRotation"];
                    transform.localScale = (Vector3)values["localScale"];
                }
                else if (target is Graphic graphic)
                {
                    graphic.color = (Color)values["color"];
                }
                else if (target is CanvasGroup canvasGroup)
                {
                    canvasGroup.alpha = (float)values["alpha"];
                }
            }
            _initialStates.Clear();
            SceneView.RepaintAll();
        }
    }
}
#endif