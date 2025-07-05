// 파일 경로: Assets/UGUIAnimationToolkit/ProgressBar/ProgressBarAnimationContext.cs

using UGUIAnimationToolkit.Core;

namespace UGUIAnimationToolkit.ProgressBar
{
    public class ProgressBarAnimationContext : UIAnimationContext
    {
        public UGUIProgressBar TargetProgressBar { get; }
        public float StartValue { get; }
        public float TargetValue { get; }

        public ProgressBarAnimationContext(UGUIProgressBar owner, float startValue, float targetValue) : base(owner)
        {
            TargetProgressBar = owner;
            StartValue = startValue;
            TargetValue = targetValue;
        }
    }
}