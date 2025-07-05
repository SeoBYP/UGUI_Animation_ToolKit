using UnityEngine;
using LitMotion;

namespace UGUIAnimationToolkit.Popup
{
    public class UIPopupAnimationContext
    {
        public RectTransform PopupRectTransform { get; }
        public CanvasGroup PopupCanvasGroup { get; }
        public CompositeMotionHandle MotionHandle { get; }

        public UIPopupAnimationContext(RectTransform rectTransform, CanvasGroup canvasGroup)
        {
            PopupRectTransform = rectTransform;
            PopupCanvasGroup = canvasGroup;
            MotionHandle = new CompositeMotionHandle();
        }
    }
}