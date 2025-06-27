using TMPro;
using LitMotion;

namespace UGUIAnimationToolkit.Text
{
    public class TextAnimationContext
    {
        public TMP_Text TargetText { get; }
        public CompositeMotionHandle MotionHandle { get; }

        public TextAnimationContext(TMP_Text targetText)
        {
            TargetText = targetText;
            MotionHandle = new CompositeMotionHandle();
        }
    }
}