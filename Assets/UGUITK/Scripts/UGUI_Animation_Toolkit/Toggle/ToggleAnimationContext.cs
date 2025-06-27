using UnityEngine.UI;
using LitMotion;

namespace UGUIAnimationToolkit.Toggle
{
    public class ToggleAnimationContext
    {
        public UGUIToggle TargetToggle { get; }
        public Graphic CheckmarkGraphic { get; } // 토글의 체크마크 이미지
        public Graphic BackgroundGraphic { get; } // 토글의 배경 이미지
        public CompositeMotionHandle MotionHandle { get; }
        public bool IsOn { get; internal set; } // 토글의 현재 상태 (On/Off)

        public ToggleAnimationContext(UGUIToggle targetToggle)
        {
            TargetToggle = targetToggle;
            CheckmarkGraphic = targetToggle.graphic;
            BackgroundGraphic = targetToggle.backgroundGraphic;
            MotionHandle = new CompositeMotionHandle();
        }
    }
}