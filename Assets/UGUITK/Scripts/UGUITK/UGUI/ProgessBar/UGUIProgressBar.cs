using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UGUIAnimationToolkit.ProgressBar
{
    [AddComponentMenu("UI/Advanced/UGUI ProgressBar", 45)]
    public class UGUIProgressBar : UIBehaviour
    {
        [System.Serializable]
        public class ProgressBarEvent : UnityEvent<float>
        {
        }

        public enum EDirection
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom,
        }

        [Header("Target Graphics")] [Tooltip("실제 값을 표시하는 기본 그래픽 (Image Type이 Filled여야 합니다)")] [SerializeField]
        private Image _fillImage;

        public Image fillImage => _fillImage;

        [Header("Value")] [Tooltip("프로그레스 바의 최소값입니다.")] [SerializeField]
        private float _minValue = 0f;

        public float MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                Set(_value);
            }
        }

        [Tooltip("프로그레스 바의 최대값입니다.")] [SerializeField]
        private float _maxValue = 1f;

        public float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                Set(_value);
            }
        }

        [Tooltip("프로그레스 바의 현재 값입니다.")] [SerializeField]
        private float _value = 1f;

        public float Value
        {
            get => _value;
            set => Set(value);
        }

        public float NormalizedValue
        {
            get
            {
                if (Mathf.Approximately(_minValue, _maxValue)) return 0;
                return Mathf.InverseLerp(_minValue, _maxValue, _value);
            }
            set => this.Value = Mathf.Lerp(_minValue, _maxValue, value);
        }

        [FormerlySerializedAs("m_Direction")] [Header("Settings")] [Tooltip("채워지는 방향입니다.")] [SerializeField]
        private EDirection _direction = EDirection.LeftToRight;

        public EDirection Direction
        {
            get => _direction;
            set
            {
                _direction = value;
                UpdateVisuals();
            }
        }
        
        [FormerlySerializedAs("m_OnValueChanged")]
        [Header("Events")]
        [SerializeField] private ProgressBarEvent _OnValueChanged = new ProgressBarEvent();
        public ProgressBarEvent OnValueChanged { get => _OnValueChanged; set => _OnValueChanged = value; }


        // [수정] ProgressBarAnimator를 직접 소유합니다.
        [Header("Animation")]
        [SerializeField] private ProgressBarAnimator animator = new ProgressBarAnimator();

        protected override void Awake()
        {
            base.Awake();
            animator.Initialize(this);
        }

        protected override void Start()
        {
            base.Start();
            UpdateVisuals();
            SetValueImmediate(_value);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            // 에디터에서 값을 변경할 때 시각적 업데이트를 즉시 반영합니다.
            if (_fillImage != null)
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
            float newValue = Mathf.Clamp(inputValue, _minValue, _maxValue);
            if (Mathf.Approximately(_value, newValue)) return;
            
            AnimateTo(sendCallback, newValue);
        }



        public void AddValue(float amount, bool sendCallback = true)
        {
            float newValue = Mathf.Clamp(Value + amount, _minValue, _maxValue);
            if (Mathf.Approximately(_value, newValue)) return;
            
            AnimateTo(sendCallback, newValue);
        }

        public void MinusValue(float amount, bool sendCallback = true)
        {
            float newValue = Mathf.Clamp(Value - amount, _minValue, _maxValue);
            if (Mathf.Approximately(_value, newValue)) return;
            
            AnimateTo(sendCallback, newValue);
        }
        
        private void AnimateTo(bool sendCallback, float newValue)
        {
            // 애니메이션 시작 전의 값을 저장합니다.
            float startValue = _value;
            
            _value = newValue;
            
            if(Application.isPlaying && animator != null)
            {
                // [수정] Animator에 시작값과 목표값을 전달하여 애니메이션 실행
                animator.Play("OnValueChange", startValue, _value);
            }
            else
            {
                SetValueImmediate(_value);
            }
            
            if (sendCallback)
            {
                _OnValueChanged.Invoke(newValue);
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
            _value = Mathf.Clamp(inputValue, _minValue, _maxValue);
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            if (_fillImage == null) return;

            // 방향에 따라 fillOrigin을 설정합니다.
            switch (_direction)
            {
                case EDirection.LeftToRight: _fillImage.fillOrigin = (int)Image.OriginHorizontal.Left; break;
                case EDirection.RightToLeft: _fillImage.fillOrigin = (int)Image.OriginHorizontal.Right; break;
                case EDirection.BottomToTop: _fillImage.fillOrigin = (int)Image.OriginVertical.Bottom; break;
                case EDirection.TopToBottom: _fillImage.fillOrigin = (int)Image.OriginVertical.Top; break;
            }

            _fillImage.fillAmount = NormalizedValue;
        }
    }
}