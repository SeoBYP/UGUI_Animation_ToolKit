using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UGUIAnimationToolkit.ProgressBar
{
    [AddComponentMenu("UI/Advanced/UI ProgressBar", 45)]
    public class UIProgressBar : UIBehaviour
    {
        [System.Serializable]
        public class ProgressBarEvent : UnityEvent<float>
        {
        }

        public enum Direction
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom,
        }

        [Header("Target Graphics")] [Tooltip("실제 값을 표시하는 기본 그래픽 (Image Type이 Filled여야 합니다)")] [SerializeField]
        private Image m_FillImage;

        public Image fillImage => m_FillImage;

        [Header("Value")] [Tooltip("프로그레스 바의 최소값입니다.")] [SerializeField]
        private float m_MinValue = 0f;

        public float minValue
        {
            get => m_MinValue;
            set
            {
                m_MinValue = value;
                Set(m_Value);
            }
        }

        [Tooltip("프로그레스 바의 최대값입니다.")] [SerializeField]
        private float m_MaxValue = 1f;

        public float maxValue
        {
            get => m_MaxValue;
            set
            {
                m_MaxValue = value;
                Set(m_Value);
            }
        }

        [Tooltip("프로그레스 바의 현재 값입니다.")] [SerializeField]
        private float m_Value = 1f;

        public float value
        {
            get => m_Value;
            set => Set(value);
        }

        public float normalizedValue
        {
            get
            {
                if (Mathf.Approximately(m_MinValue, m_MaxValue)) return 0;
                return Mathf.InverseLerp(m_MinValue, m_MaxValue, m_Value);
            }
            set => this.value = Mathf.Lerp(m_MinValue, m_MaxValue, value);
        }

        [Header("Settings")] [Tooltip("채워지는 방향입니다.")] [SerializeField]
        private Direction m_Direction = Direction.LeftToRight;

        public Direction direction
        {
            get => m_Direction;
            set
            {
                m_Direction = value;
                UpdateVisuals();
            }
        }

        [Header("Events")] [Tooltip("값이 변경될 때 호출될 이벤트입니다. (현재 값을 매개변수로 전달)")] [SerializeField]
        private ProgressBarEvent m_OnValueChanged = new ProgressBarEvent();

        public ProgressBarEvent onValueChanged
        {
            get => m_OnValueChanged;
            set => m_OnValueChanged = value;
        }

        [Header("Animation")] [Tooltip("값이 변경될 때 재생될 애니메이션 시퀀스입니다.")] [SerializeField]
        private ProgressBarAnimationSequence m_AnimationSequence = new();

        private ProgressBarAnimationContext _context;

        protected override void Awake()
        {
            base.Awake();
            _context = new ProgressBarAnimationContext(this);
        }

        protected override void Start()
        {
            base.Start();
            UpdateVisuals();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            // 에디터에서 값을 변경할 때 시각적 업데이트를 즉시 반영합니다.
            if (m_FillImage != null)
            {
                UpdateVisuals();
            }
        }
#endif

        /// <summary>
        /// 프로그레스 바의 값을 애니메이션과 함께 설정합니다.
        /// </summary>
        public void SetValue(float inputValue, bool sendCallback = true)
        {
            float newValue = Mathf.Clamp(inputValue, m_MinValue, m_MaxValue);
            if (Mathf.Approximately(m_Value, newValue)) return;

            AnimateTo(newValue);

            if (sendCallback)
            {
                m_OnValueChanged.Invoke(newValue);
            }
        }

        public void AddValue(float amount, bool sendCallback = true)
        {
            float newValue = Mathf.Clamp(value + amount, m_MinValue, m_MaxValue);
            if (Mathf.Approximately(m_Value, newValue)) return;
            AnimateTo(newValue);

            if (sendCallback)
            {
                m_OnValueChanged.Invoke(newValue);
            }
        }
        
        public void MinusValue(float amount, bool sendCallback = true)
        {
            float newValue = Mathf.Clamp(value - amount, m_MinValue, m_MaxValue);
            if (Mathf.Approximately(m_Value, newValue)) return;
            AnimateTo(newValue);

            if (sendCallback)
            {
                m_OnValueChanged.Invoke(newValue);
            }
        }
        
        private void Set(float inputValue)
        {
            SetValue(inputValue, true);
        }

        /// <summary>
        /// 프로그레스 바의 값을 즉시 설정합니다. (애니메이션 없음)
        /// </summary>
        public void SetValueImmediate(float inputValue)
        {
            m_Value = Mathf.Clamp(inputValue, m_MinValue, m_MaxValue);
            UpdateVisuals();
        }

        private void AnimateTo(float newTargetValue)
        {
            _context.StartValue = normalizedValue; // 이전 정규화된 값
            _context.TargetValue = Mathf.InverseLerp(m_MinValue, m_MaxValue, newTargetValue); // 목표 정규화된 값

            // 현재 값을 새로운 목표 값으로 업데이트합니다.
            m_Value = newTargetValue;

            _ = m_AnimationSequence.PlayAsync(_context);
        }

        private void UpdateVisuals()
        {
            if (m_FillImage == null) return;

            // 방향에 따라 fillOrigin을 설정합니다.
            switch (m_Direction)
            {
                case Direction.LeftToRight: m_FillImage.fillOrigin = (int)Image.OriginHorizontal.Left; break;
                case Direction.RightToLeft: m_FillImage.fillOrigin = (int)Image.OriginHorizontal.Right; break;
                case Direction.BottomToTop: m_FillImage.fillOrigin = (int)Image.OriginVertical.Bottom; break;
                case Direction.TopToBottom: m_FillImage.fillOrigin = (int)Image.OriginVertical.Top; break;
            }

            m_FillImage.fillAmount = normalizedValue;
        }
    }
}