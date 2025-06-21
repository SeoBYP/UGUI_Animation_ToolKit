using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

namespace UGUIAnimationToolkit.Text
{
    [AddComponentMenu("UI/Advanced/UGUI Text Animator", 40)]
    [RequireComponent(typeof(TMP_Text))]
    public class TextAnimator : MonoBehaviour
    {
 
        [Header("Animation Sequences")] 
        [Tooltip("컴포넌트가 활성화될 때 재생될 애니메이션입니다.")] [SerializeField]
        private TextAnimationSequence onPlaySequence = new();

        // 여기에 OnClick, OnShow 등 다양한 시퀀스를 추가할 수 있습니다.
        // [SerializeField] private TextAnimationSequence customSequence = new();

        private TextAnimationContext _context;
        private TMP_Text _targetText;

        private void Awake()
        {
            _targetText = GetComponent<TMP_Text>();
            _context = new TextAnimationContext(_targetText);
        }

        private void OnEnable()
        {
            if (onPlaySequence != null && onPlaySequence.modules.Count > 0)
            {
                _ = onPlaySequence.PlayAsync(_context);
            }
        }

        /// <summary>
        /// 코드에서 직접 애니메이션을 재생할 때 사용하는 public 메서드입니다.
        /// </summary>
        public void Play()
        {
            if (onPlaySequence != null)
            {
                _ = onPlaySequence.PlayAsync(_context);
            }
        }
    }
}