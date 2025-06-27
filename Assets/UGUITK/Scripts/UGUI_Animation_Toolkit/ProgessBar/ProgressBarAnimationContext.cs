using LitMotion;

namespace UGUIAnimationToolkit.ProgressBar
{
    public class ProgressBarAnimationContext
    {
        public UIProgressBar TargetProgressBar { get; }
        public CompositeMotionHandle MotionHandle { get; }
        public float StartValue { get; internal set; }
        public float TargetValue { get; internal set; }

        public ProgressBarAnimationContext(UIProgressBar targetProgressBar)
        {
            TargetProgressBar = targetProgressBar;
            MotionHandle = new CompositeMotionHandle();
        }
    }
}