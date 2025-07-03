using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIAnimationToolkit.Core
{
    [Serializable]
    public abstract class UGUIAnimator
    {
        [Tooltip("이 애니메이터가 정의하는 모든 애니메이션 이벤트 리스트입니다.")] [SerializeField]
        protected List<UIAnimationEvent> animationEvents = new();

        [NonSerialized] private Dictionary<string, UIAnimationEvent> _eventMap;
        [NonSerialized] private bool _isInitialized = false;
        [NonSerialized] private UIAnimationContext _ctx;

        /// <summary>
        /// 컴포넌트를 받아 컨텍스트를 생성합니다.
        /// </summary>
        /// <param name="owner"></param>
        public void Initialize(Component owner)
        {
            if (_isInitialized)
                return;
            _ctx = new UIAnimationContext(owner);
            _eventMap = new Dictionary<string, UIAnimationEvent>();
            foreach (var animEvent in animationEvents)
            {
                if (!string.IsNullOrWhiteSpace(animEvent.eventName))
                {
                    _eventMap.Add(animEvent.eventName, animEvent);
                }
            }

            _isInitialized = true;
        }

        public void Play(string eventName, PointerEventData eventData = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError("[UGUIAnimator] Animator is not initialized. Call Initialize() in Awake or OnEnable");
                return;
            }

            if (_eventMap.TryGetValue(eventName, out var animEvent))
            {
                _ctx.PointerEventData = eventData;
                _ = animEvent.sequence.PlayAsync(_ctx)
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

        public void Revert(string eventName, PointerEventData eventData = null)
        {
            if (!_isInitialized)
                return;
            if (_eventMap.TryGetValue(eventName, out var animEvent))
            {
                _ctx.PointerEventData = eventData;
                _ = animEvent.sequence.RevertAsync(_ctx);
            }
        }
        
       
    }
}