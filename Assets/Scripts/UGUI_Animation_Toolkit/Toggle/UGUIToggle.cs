using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
// UGUIAnimationToolkit.Toggle 네임스페이스가 없다면 UGUIAnimationToolkit으로 변경해야 할 수 있습니다.
using UGUIAnimationToolkit.Toggle;

namespace UGUIAnimationToolkit
{
    [AddComponentMenu("UI/Advanced/UGUI Toggle", 31)]
    [RequireComponent(typeof(RectTransform))]
    public class UGUIToggle : UIBehaviour, IPointerClickHandler, ISubmitHandler, ICanvasElement,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [System.Serializable]
        public class ToggleEvent : UnityEvent<bool>
        {
        }

        [Header("Toggle Settings")] [Tooltip("토글을 상호작용 가능하게 할지 여부")] [SerializeField]
        private bool m_Interactable = true;

        [Tooltip("토글의 On/Off 상태를 표시할 메인 그래픽 (예: 체크마크)")]
        public Graphic graphic;

        // ▼▼▼▼▼ 이 필드가 누락되어 에러가 발생했습니다. ▼▼▼▼▼
        [Tooltip("토글의 배경으로 사용될 그래픽입니다.")] public Graphic backgroundGraphic;
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        [Tooltip("이 토글이 속한 그룹입니다. (선택 사항)")] [SerializeField]
        private UGUIToggleGroup m_Group;

        [Header("Value")] [Tooltip("토글의 현재 On/Off 상태")] [SerializeField]
        private bool m_IsOn;

        [Tooltip("토글의 값이 변경될 때 호출될 이벤트")] public ToggleEvent onValueChanged = new ToggleEvent();

        [Header("Animation Sequences")] [SerializeField]
        private ToggleAnimationSequence onSequence = new();

        [SerializeField] private ToggleAnimationSequence offSequence = new();

        private ToggleAnimationContext _context;

        // --- Public 프로퍼티 ---
        public UGUIToggleGroup group
        {
            get => m_Group;
            set
            {
                if (m_Group == value) return;
                SetToggleGroup(value, true);
                PlayEffect(true);
            }
        }

        public bool isOn
        {
            get => m_IsOn;
            set => Set(value);
        }

        public bool interactable
        {
            get => m_Interactable;
            set => m_Interactable = value;
        }

        // --- 생성자 및 생명주기 ---
        protected UGUIToggle()
        {
        }

        protected override void Awake()
        {
            base.Awake();
            // [수정] 컨텍스트 생성 시 backgroundGraphic도 함께 전달합니다.
            _context = new ToggleAnimationContext(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetToggleGroup(m_Group, false);
            PlayEffect(true);
        }

        protected override void Start()
        {
            base.Start();
            PlayEffect(true);
        }

        protected override void OnDisable()
        {
            SetToggleGroup(null, false);
            base.OnDisable();
        }

        // --- 핵심 로직 ---
        public void SetIsOnWithoutNotify(bool value)
        {
            Set(value, false);
        }

        private void Set(bool value, bool sendCallback = true)
        {
            if (m_IsOn == value) return;
            m_IsOn = value;

            if (m_Group != null && m_Group.isActiveAndEnabled && IsActive())
            {
                if (m_IsOn || (!m_Group.AnyTogglesOn() && !m_Group.allowSwitchOff))
                {
                    m_IsOn = true;
                    m_Group.NotifyToggleOn(this, sendCallback);
                }
            }

            PlayEffect(false);

            if (sendCallback)
            {
                onValueChanged.Invoke(m_IsOn);
            }
        }

        private void PlayEffect(bool instant)
        {
            if (_context == null) return;
            _context.IsOn = m_IsOn;
            var sequence = m_IsOn ? onSequence : offSequence;
            if (sequence == null) return;

            var playTask = sequence.PlayAsync(_context);

            if (instant)
            {
                _context.MotionHandle.Complete();
            }
        }

        private void InternalToggle()
        {
            if (!IsActive() || !IsInteractable())
                return;
            isOn = !isOn;
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

        private void SetToggleGroup(UGUIToggleGroup newGroup, bool setMemberValue)
        {
            if (m_Group != null) m_Group.UnregisterToggle(this);
            if (setMemberValue) m_Group = newGroup;
            if (newGroup != null && IsActive()) newGroup.RegisterToggle(this);
            if (newGroup != null && m_IsOn && IsActive()) newGroup.NotifyToggleOn(this, false);
        }

        // --- ICanvasElement & 이벤트 핸들러 ---
        public virtual void Rebuild(CanvasUpdate executing)
        {
        }

        public virtual void LayoutComplete()
        {
        }

        public virtual void GraphicUpdateComplete()
        {
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            InternalToggle();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            InternalToggle();
        }

        // IPointerEnterHandler 등을 구현하려면 아래 메서드들도 필요합니다.
        // 현재 코드에는 없어서 추가합니다.
        public void OnPointerEnter(PointerEventData eventData)
        {
            /* 필요 시 로직 추가 */
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            /* 필요 시 로직 추가 */
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            /* 필요 시 로직 추가 */
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            /* 필요 시 로직 추가 */
        }
    }
}