using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIAnimationToolkit
{
    [AddComponentMenu("UI/Advanced/UGUI Button", 30)]
    [RequireComponent(typeof(RectTransform))]
    public class UGUIButton : UIBehaviour,
        IPointerClickHandler, ISubmitHandler,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [System.Serializable]
        public class ButtonClickedEvent : UnityEvent
        {
        }

        [Tooltip("버튼을 상호작용 가능하게 할지 여부")] [SerializeField]
        private bool m_Interactable = true;

        [Tooltip("클릭 이벤트")] [SerializeField] private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

        [Tooltip("Throttle: 지정된 시간(초) 동안 중복 클릭을 무시합니다. (0 = 비활성화)")] [SerializeField]
        private float m_ThrottleDuration = 0f;

        [Tooltip("Debounce: 마지막 클릭 후 지정된 시간(초)이 지나야 이벤트가 발생합니다. (0 = 비활성화)")] [SerializeField]
        private float m_DebounceDuration = 0f;

        [Header("Animation Sequences")] [SerializeField]
        private ButtonAnimationSequence hoverSequence = new();

        [SerializeField] private ButtonAnimationSequence clickSequence = new();

        private readonly UIButtonAnimationContext _context = new();
        private bool _isThrottled = false;
        private System.Threading.CancellationTokenSource _debounceCts;

// --- Public Properties ---
        public ButtonClickedEvent onClick
        {
            get => m_OnClick;
            set => m_OnClick = value;
        }

        public bool interactable
        {
            get => m_Interactable;
            set => m_Interactable = value;
        }

        // --- Lifecycle & Interaction ---
        protected override void OnDestroy()
        {
            base.OnDestroy();
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
        }

        // Selectable의 IsInteractable 로직을 직접 구현합니다.
        public virtual bool IsInteractable()
        {
            if (!m_Interactable) return false;

            var groups = GetComponentsInParent<CanvasGroup>();
            for (var i = 0; i < groups.Length; i++)
            {
                if (!groups[i].interactable) return false;
                if (groups[i].ignoreParentGroups) break;
            }

            return true;
        }

        private void Press()
        {
            if (!IsActive() || !IsInteractable())
                return;
            // Throttle 로직
            if (m_ThrottleDuration > 0)
            {
                if (_isThrottled) return;
                _isThrottled = true;
                int duration = (int)(m_ThrottleDuration * 1000);
                UniTask.Delay(duration)
                    .ContinueWith(() => _isThrottled = false).Forget();
            }

            // Debounce 로직
            if (m_DebounceDuration > 0)
            {
                _debounceCts?.Cancel();
                _debounceCts?.Dispose();
                _debounceCts = new System.Threading.CancellationTokenSource();
                DebouncedPress(_debounceCts.Token).Forget();
                return; // Debounce는 즉시 실행되지 않음
            }

            // 이벤트 실행
            m_OnClick.Invoke();
        }

        private async UniTaskVoid DebouncedPress(System.Threading.CancellationToken token)
        {
            await UniTask.Delay((int)(m_DebounceDuration * 1000), cancellationToken: token);
            if (token.IsCancellationRequested) return;
            m_OnClick.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) return;
            PlayAnimation(hoverSequence, ButtonAnimationEventType.Enter, eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) return;
            PlayAnimation(hoverSequence, ButtonAnimationEventType.Exit, eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) return;
            PlayAnimation(clickSequence, ButtonAnimationEventType.Down, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) return;
            PlayAnimation(clickSequence, ButtonAnimationEventType.Up, eventData);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            Press();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Press();
        }

        private void PlayAnimation(ButtonAnimationSequence sequence, ButtonAnimationEventType eventType,
            PointerEventData eventData)
        {
            if (sequence == null) return;
            _context.EventType = eventType;
            _context.PointerEventData = eventData;
            switch (eventType)
            {
                case ButtonAnimationEventType.Enter:
                case ButtonAnimationEventType.Down:
                    _ = sequence.PlayAsync(_context);
                    break;
                case ButtonAnimationEventType.Up:
                case ButtonAnimationEventType.Exit:
                    _ = sequence.RevertAsync(_context);
                    break;
            }
        }
    }
}