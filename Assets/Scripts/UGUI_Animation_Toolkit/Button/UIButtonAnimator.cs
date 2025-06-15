using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIAnimationToolkit
{
    public class UIButtonAnimator : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [Header("Sequences")]
        [SerializeField] private AnimationSequence enterSequence = new();
        [SerializeField] private AnimationSequence exitSequence = new();
        [SerializeField] private AnimationSequence downSequence = new();
        [SerializeField] private AnimationSequence upSequence = new();

        private readonly UIButtonAnimationContext _context = new();

        public void OnPointerEnter(PointerEventData eventData) => Play(enterSequence, ButtonAnimationEventType.Enter, eventData);
        public void OnPointerExit(PointerEventData eventData) => Play(exitSequence, ButtonAnimationEventType.Exit, eventData);
        public void OnPointerDown(PointerEventData eventData) => Play(downSequence, ButtonAnimationEventType.Down, eventData);
        public void OnPointerUp(PointerEventData eventData) => Play(upSequence, ButtonAnimationEventType.Up, eventData);

        private void Play(AnimationSequence sequence, ButtonAnimationEventType eventType, PointerEventData eventData)
        {
            if (sequence == null) return;
            _context.EventType = eventType;
            _context.PointerEventData = eventData;
            _ = sequence.PlayAsync(_context);
        }
    }
}