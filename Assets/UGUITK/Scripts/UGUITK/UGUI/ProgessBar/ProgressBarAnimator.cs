using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.ProgressBar
{
    [Serializable]
    public class ProgressBarAnimator : UGUIAnimator
    {
        /// <summary>
        /// 생성자: ButtonAnimator가 생성될 때 기본 이벤트를 설정합니다.
        /// </summary>
        public ProgressBarAnimator()
        {
            // [추가] animationEvents 리스트가 비어있거나 없을 때만 기본값을 추가합니다.
            // 이렇게 하면 이미 저장된 데이터가 있는 경우 덮어쓰지 않습니다.
            if (animationEvents == null || animationEvents.Count == 0)
            {
                animationEvents = new List<UIAnimationEvent>
                {
                    new UIAnimationEvent { eventName = ProgressBarConstants.OnValueChange },
                };
            }
        }
        
        public void Play(string eventName, float startValue, float targetValue)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[UGUIAnimator] Animator is not initialized.");
                return;
            }

            if (_eventMap.TryGetValue(eventName, out var animEvent))
            {
                // 새로 만든 ProgressBarAnimationContext를 사용합니다.
                var context = new ProgressBarAnimationContext(_owner as UGUIProgressBar, startValue, targetValue);
                
                // 생성된 컨텍스트로 애니메이션을 재생합니다.
                _ = animEvent.sequence.PlayAsync(context)
                    .ContinueWith(() =>
                    {
                        animEvent?.onCompleted?.Invoke();
                    });
            }
            else
            {
                Debug.LogError($"[UGUIAnimator] {eventName} Can't find event.");
            }
        }
    }
}