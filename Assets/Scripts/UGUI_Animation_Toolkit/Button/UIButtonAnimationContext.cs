using LitMotion;
using TMPro;
using UGUI_Animation_Toolkit.Button;
using UnityEngine;

namespace UGUIAnimationToolkit
{
    public class UIButtonAnimationContext
    {
        public RectTransform RectTransform { get; }
        public TMP_Text Label { get; }
        public CompositeMotionHandle MotionHandle { get; }
        public ButtonAnimationEventType EventType { get; internal set; }
        
        public UIButtonAnimationContext(RectTransform rect, TMP_Text label)
        {
            RectTransform = rect;
            Label = label;
            MotionHandle = new CompositeMotionHandle();
        }
    }
}