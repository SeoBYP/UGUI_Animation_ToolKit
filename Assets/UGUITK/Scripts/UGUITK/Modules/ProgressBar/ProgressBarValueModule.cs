using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.ProgressBar.Modules
{
    [ModuleCategory("ProgressBar/Effect", Order = 20)]
    [Serializable]
    public class ProgressBarValueModule : ProgressBarAnimationModule
    {
        [Header("Animation Settings")]
        public UGUIProgressBar TargetProgressBar;
        public float Duration = 0.5f;
        public Ease Ease = Ease.OutQuad;
        
        public override UniTask AnimateAsync(ProgressBarAnimationContext ctx)
        {
            var fillImage = ctx.TargetProgressBar.fillImage;
            if (fillImage == null) return UniTask.CompletedTask;

            // 정규화된 값으로 변환하여 애니메이션합니다.
            var from = ctx.StartValue / ctx.TargetProgressBar.MaxValue;
            var to = ctx.TargetValue / ctx.TargetProgressBar.MaxValue;
            
            return LMotion.Create(from, to, Duration)
                .WithEase(Ease)
                .BindToFillAmount(fillImage)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(ProgressBarAnimationContext ctx)
        {
            return UniTask.CompletedTask;
        }
    }
}