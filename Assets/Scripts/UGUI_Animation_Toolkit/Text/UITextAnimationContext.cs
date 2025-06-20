using TMPro;
using LitMotion;

namespace UGUIAnimationToolkit.Text
{
    public class UITextAnimationContext
    {
        public TMP_Text TargetText { get; }
        public CompositeMotionHandle MotionHandle { get; }

        public UITextAnimationContext(TMP_Text targetText)
        {
            TargetText = targetText;
            MotionHandle = new CompositeMotionHandle();
        }
    }
}