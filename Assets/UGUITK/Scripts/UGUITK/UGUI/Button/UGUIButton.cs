using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;


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

        [Header("Button Settings")] [Tooltip("버튼을 상호작용 가능하게 할지 여부")] [SerializeField]
        private bool m_Interactable = true;

        [Tooltip("클릭 이벤트")] [SerializeField] private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

        [Header("Advanced Settings")] [Tooltip("Throttle: 지정된 시간(초) 동안 중복 클릭을 무시합니다. (0 = 비활성화)")] [SerializeField]
        private float m_ThrottleDuration = 0f;

        [Tooltip("Debounce: 마지막 클릭 후 지정된 시간(초)이 지나야 이벤트가 발생합니다. (0 = 비활성화)")] [SerializeField]
        private float m_DebounceDuration = 0f;

        // [수정] AnimationSequence 필드 대신, ButtonAnimator를 직접 소유합니다.
        [Header("Animations")] [Tooltip("버튼의 애니메이션 로직을 포함합니다.")] [SerializeField]
        private ButtonAnimator animator = new ButtonAnimator();

        private bool _isThrottled = false;
        private System.Threading.CancellationTokenSource _debounceCts;

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

        protected override void Awake()
        {
            base.Awake();
            // [추가] 컴포넌트가 활성화될 때 애니메이터를 초기화합니다.
            animator.Initialize(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _debounceCts?.Cancel();
            _debounceCts?.Dispose();
        }

        public virtual bool IsInteractable()
        {
            if (!m_Interactable) return false;
            var groups = GetComponentsInParent<CanvasGroup>();
            foreach (var group in groups)
            {
                if (!group.interactable) return false;
                if (group.ignoreParentGroups) break;
            }

            return true;
        }

        private void Press()
        {
            if (!IsActive() || !IsInteractable()) return;
            if (m_ThrottleDuration > 0)
            {
                if (_isThrottled) return;
                _isThrottled = true;
                UniTask.Delay((int)(m_ThrottleDuration * 1000)).ContinueWith(() => _isThrottled = false).Forget();
            }

            if (m_DebounceDuration > 0)
            {
                _debounceCts?.Cancel();
                _debounceCts?.Dispose();
                _debounceCts = new System.Threading.CancellationTokenSource();
                DebouncedPress(_debounceCts.Token).Forget();
                return;
            }

            m_OnClick.Invoke();
        }

        private async UniTaskVoid DebouncedPress(System.Threading.CancellationToken token)
        {
            try
            {
                await UniTask.Delay((int)(m_DebounceDuration * 1000), cancellationToken: token);
                m_OnClick.Invoke();
            }
            catch (System.OperationCanceledException)
            {
            }
        }

        // --- 이벤트 핸들러 수정 ---
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsInteractable()) return;
            animator.Play("OnHover", eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!IsInteractable()) return;
            animator.Revert("OnHover", eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsInteractable()) return;
            animator.Play("OnClick", eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsInteractable()) return;
            animator.Revert("OnClick", eventData);
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            Press();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Press();
        }
    }
}