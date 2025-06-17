using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIAnimationToolkit
{
    public class UIButtonAnimator : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [Header("Sequences")] [SerializeField] private AnimationSequence hoverSequence = new();

        [SerializeField] private AnimationSequence clickSequence = new();

        private readonly UIButtonAnimationContext _context = new();

        public void OnPointerEnter(PointerEventData eventData) =>
            Play(hoverSequence, ButtonAnimationEventType.Enter, eventData);

        public void OnPointerExit(PointerEventData eventData) =>
            Play(hoverSequence, ButtonAnimationEventType.Exit, eventData);

        public void OnPointerDown(PointerEventData eventData) =>
            Play(clickSequence, ButtonAnimationEventType.Down, eventData);

        public void OnPointerUp(PointerEventData eventData) =>
            Play(clickSequence, ButtonAnimationEventType.Up, eventData);

        private void Play(AnimationSequence sequence, ButtonAnimationEventType eventType, PointerEventData eventData)
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