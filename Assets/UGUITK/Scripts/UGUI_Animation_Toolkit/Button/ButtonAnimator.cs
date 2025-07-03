using System;
using System.Collections.Generic;
using UGUIAnimationToolkit.Core;

namespace UGUIAnimationToolkit
{
    [Serializable]
    public class ButtonAnimator : UGUIAnimator
    {
        /// <summary>
        /// 생성자: ButtonAnimator가 생성될 때 기본 이벤트를 설정합니다.
        /// </summary>
        public ButtonAnimator()
        {
            // [추가] animationEvents 리스트가 비어있거나 없을 때만 기본값을 추가합니다.
            // 이렇게 하면 이미 저장된 데이터가 있는 경우 덮어쓰지 않습니다.
            if (animationEvents == null || animationEvents.Count == 0)
            {
                animationEvents = new List<UIAnimationEvent>
                {
                    new UIAnimationEvent { eventName = "OnHover" },
                    new UIAnimationEvent { eventName = "OnClick" }
                };
            }
        }
    }
}