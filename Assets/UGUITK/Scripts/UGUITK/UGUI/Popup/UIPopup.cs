using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace UGUIAnimationToolkit.Popup
{
    [AddComponentMenu("UI/Advanced/UGUI Popup", 50)]
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPopup : MonoBehaviour
    {
        [Header("Animation Sequences")]
        [SerializeField] private PopupAnimationSequence showSequence = new();
        [SerializeField] private PopupAnimationSequence hideSequence = new();

        private CanvasGroup _canvasGroup;
        private UIPopupAnimationContext _context;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _context = new UIPopupAnimationContext(transform as RectTransform, _canvasGroup);
        }

        /// <summary>
        /// 팝업을 나타나게 합니다.
        /// </summary>
        public async UniTask Show()
        {
            // 상호작용 활성화
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            
            await showSequence.PlayAsync(_context);
        }

        /// <summary>
        /// 팝업을 사라지게 합니다.
        /// </summary>
        public async UniTask Hide()
        {
            // 상호작용 비활성화
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            await hideSequence.PlayAsync(_context);
        }

        private void OnEnable()
        {
            _ = Show();
        }

        private void OnDisable()
        {
            _ = Hide();
        }
    }
}