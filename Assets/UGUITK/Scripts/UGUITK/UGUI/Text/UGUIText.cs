using UnityEngine;
using TMPro;
using UGUIAnimationToolkit.Core;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UGUIAnimationToolkit.Text
{
    [AddComponentMenu("UI/Advanced/UGUI Text", 40)]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UGUIText : UIBehaviour
    {
        [System.Serializable]
        public class TextChangedEvent : UnityEvent<string>
        {
        }

        [FormerlySerializedAs("textMeshProUGUI")]
        [FormerlySerializedAs("_textComponent")]
        [FormerlySerializedAs("m_TextComponent")]
        [Header("Target Graphics")]
        [Tooltip("실제 텍스트를 표시하는 TextMeshProUGUI 컴포넌트입니다.")]
        [SerializeField]
        private TextMeshProUGUI _textMeshProUGUI;

        public TextMeshProUGUI TextMeshProUGUI => _textMeshProUGUI ??= GetComponent<TextMeshProUGUI>();

        [Header("Value")] [Tooltip("현재 표시되고 있는 텍스트 값입니다.")] [SerializeField, TextArea(3, 10)]
        private string _text;

        public string Text
        {
            get => _text;
            set => SetText(value);
        }

        [Header("Animation")] [SerializeField] private TextAnimator animator = new TextAnimator();

        [Header("Events")] [SerializeField] private TextChangedEvent _onTextChanged = new TextChangedEvent();

        public TextChangedEvent OnTextChanged
        {
            get => _onTextChanged;
            set => _onTextChanged = value;
        }
        
#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            // 컴포넌트가 추가되는 즉시 TextMeshProUGUI를 찾아 할당합니다.
            _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            UpdateVisuals();
        }
#endif
        protected override void Awake()
        {
            base.Awake();
            animator.Initialize(this);
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
            if (_textMeshProUGUI == null)
            {
                _textMeshProUGUI = GetComponent<TextMeshProUGUI>();
            }
            UpdateVisuals();
        }
#endif

        /// <summary>
        /// 텍스트를 애니메이션과 함께 설정합니다.
        /// </summary>
        public void SetText(string newText, bool sendCallback = true)
        {
            if (string.Equals(_text, newText)) return;
            ;
            // 애니메이션 시작 전의 텍스트를 저장합니다.
            string startText = _text;

            // UGUIText의 최종 값은 미리 업데이트합니다.
            _text = newText;

            // 플레이 중일 때만 애니메이터를 호출합니다.
            if (Application.isPlaying && animator != null)
            {
                // 수정된 Animator의 Play 메서드를 호출하여 시작/목표 텍스트를 전달합니다.
                animator.Play(TextConstants.OnTextChange, startText, newText);
            }
            else
            {
                // 플레이 중이 아니면 즉시 시각적 업데이트
                UpdateVisuals();
            }

            if (sendCallback)
            {
                _onTextChanged.Invoke(newText);
            }
        }

        /// <summary>
        /// 텍스트를 즉시 설정합니다. (애니메이션 없음)
        /// </summary>
        public void SetTextImmediate(string newText)
        {
            _text = newText;
            UpdateVisuals();
        }

        /// <summary>
        /// 실제 TextMeshProUGUI 컴포넌트에 값을 적용합니다.
        /// </summary>
        private void UpdateVisuals()
        {
            if (_textMeshProUGUI != null)
            {
                _textMeshProUGUI.text = _text;
            }
        }
    }
}