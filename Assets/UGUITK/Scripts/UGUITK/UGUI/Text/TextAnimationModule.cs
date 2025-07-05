// 파일 경로: Assets/UGUIAnimationToolkit/Text/TextAnimationModule.cs

using System;
using Cysharp.Threading.Tasks;
using UGUIAnimationToolkit.Core;

namespace UGUIAnimationToolkit.Text
{
    [Serializable]
    public abstract class TextAnimationModule : UIAnimationModule
    {
        // UIAnimationContext를 받으면 TextAnimationContext로 캐스팅하여 하위 클래스로 전달
        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            if (ctx is not TextAnimationContext context)
                throw new ArgumentException("Context must be of type TextAnimationContext.", nameof(ctx));
            return AnimateAsync(context);
        }

        public abstract UniTask AnimateAsync(TextAnimationContext ctx);

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            if (ctx is not TextAnimationContext context)
                throw new ArgumentException("Context must be of type TextAnimationContext.", nameof(ctx));
            return RevertAsync(context);
        }

        public abstract UniTask RevertAsync(TextAnimationContext ctx);
    }
}