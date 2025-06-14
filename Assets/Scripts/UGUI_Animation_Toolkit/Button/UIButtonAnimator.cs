using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace UGUIAnimationToolkit
{
    [RequireComponent(typeof(RectTransform))]
    public class UIButtonAnimator : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [Header("Components")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TMP_Text label;

        [Header("Sequences")]
        [SerializeField] private AnimationSequence enterSequence = new AnimationSequence();
        [SerializeField] private AnimationSequence exitSequence = new AnimationSequence();
        [SerializeField] private AnimationSequence downSequence = new AnimationSequence();
        [SerializeField] private AnimationSequence upSequence = new AnimationSequence();

        private UIButtonAnimationContext _context;

        private void Reset()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        }

        private void Awake()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (label == null) label = GetComponentInChildren<TMP_Text>();
            
            _context = new UIButtonAnimationContext(rectTransform, label);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _context.EventType = ButtonAnimationEventType.Enter;
            _ = enterSequence?.PlayAsync(_context);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _context.EventType = ButtonAnimationEventType.Exit;
            _ = exitSequence?.PlayAsync(_context);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _context.EventType = ButtonAnimationEventType.Down;
            _ = downSequence?.PlayAsync(_context);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _context.EventType = ButtonAnimationEventType.Up;
            _ = upSequence?.PlayAsync(_context);
        }
    }
}