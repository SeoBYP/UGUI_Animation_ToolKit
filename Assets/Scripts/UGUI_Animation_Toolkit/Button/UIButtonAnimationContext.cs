using LitMotion;
using TMPro;
using UGUI_Animation_Toolkit.Button;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIAnimationToolkit
{
    public class UIButtonAnimationContext
    {
        public CompositeMotionHandle MotionHandle { get; } = new();
        public ButtonAnimationEventType EventType { get; internal set; }
        public PointerEventData PointerEventData { get; internal set; }
    }
}