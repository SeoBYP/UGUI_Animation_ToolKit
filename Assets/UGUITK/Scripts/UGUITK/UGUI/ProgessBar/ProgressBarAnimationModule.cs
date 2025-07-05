// 파일 경로: Assets/UGUIAnimationToolkit/ProgressBar/ProgressBarAnimationModule.cs

using System;
using Cysharp.Threading.Tasks;
using UGUIAnimationToolkit.Core;

namespace UGUIAnimationToolkit.ProgressBar
{
    [Serializable]
    public abstract class ProgressBarAnimationModule : UIAnimationModule
    {
        // UIAnimationContext를 받으면 ProgressBarAnimationContext로 캐스팅하여 하위 클래스로 전달
        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            if (ctx is not ProgressBarAnimationContext context)
                throw new ArgumentException("Context must be of type ProgressBarAnimationContext.", nameof(ctx));
            return AnimateAsync(context);
        }

        public abstract UniTask AnimateAsync(ProgressBarAnimationContext ctx);

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            if (ctx is not ProgressBarAnimationContext context)
                throw new ArgumentException("Context must be of type ProgressBarAnimationContext.", nameof(ctx));
            return RevertAsync(context);
        }

        public abstract UniTask RevertAsync(ProgressBarAnimationContext ctx);
    }
}