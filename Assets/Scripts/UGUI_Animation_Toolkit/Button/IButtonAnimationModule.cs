using UnityEngine.EventSystems;

namespace UGUIAnimationToolkit.Button
{
    // 모듈 인터페이스
    public interface IButtonAnimationModule
    {
        void AnimateEnter(UIButtonAnimationContext ctx);
        void AnimateExit(UIButtonAnimationContext ctx);
        void AnimateDown(UIButtonAnimationContext ctx);
        void AnimateUp(UIButtonAnimationContext ctx);
    }
}