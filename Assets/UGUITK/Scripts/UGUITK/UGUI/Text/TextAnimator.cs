using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.Text
{
    [Serializable]
    public class TextAnimator : UGUIAnimator
    {
        public TextAnimator()
        {
            // [추가] animationEvents 리스트가 비어있거나 없을 때만 기본값을 추가합니다.
            if (animationEvents == null || animationEvents.Count == 0)
            {
                animationEvents = new List<UIAnimationEvent>
                {
                    new UIAnimationEvent { eventName = TextConstants.OnTextChange },
                };
            }
        }
        
        // 시작/목표 텍스트를 받는 새로운 Play 메서드
        public void Play(string eventName, string startText, string targetText)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[UGUIAnimator] Animator is not initialized.");
                return;
            }

            if (_eventMap.TryGetValue(eventName, out var animEvent))
            {
                // 수정된 TextAnimationContext를 생성합니다.
                var context = new TextAnimationContext(_owner as UGUIText, startText, targetText);
                
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