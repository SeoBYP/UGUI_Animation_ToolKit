using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.ProgressBar.Modules
{
    [ModuleCategory("ProgressBar/Effect", Order = 20)]
    [Serializable]
    public class ProgressBarValueModule : ProgressBarAnimationModule
    {
        [Header("Animation Settings")]
        public float Duration = 0.5f;
        public Ease Ease = Ease.OutQuad;
        
        public override UniTask AnimateAsync(ProgressBarAnimationContext ctx)
        {
            var fillImage = ctx.TargetProgressBar.fillImage;
            if (fillImage == null) return UniTask.CompletedTask;
            
            return LMotion.Create(ctx.StartValue, ctx.TargetValue, Duration)
                .WithEase(Ease)
                .BindToFillAmount(fillImage)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}