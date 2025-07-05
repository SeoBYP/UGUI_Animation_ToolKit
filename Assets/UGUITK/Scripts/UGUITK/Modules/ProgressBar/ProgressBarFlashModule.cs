using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAnimationToolkit.ProgressBar.Modules
{
    [ModuleCategory("ProgressBar/Effect", Order = 31)]
    [Serializable]
    public class ProgressBarFlashModule : ProgressBarAnimationModule
    {
        [Header("Animation Settings")] [Tooltip("반짝일 때의 색상입니다.")]
        public Color FlashColor = Color.white;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutQuad;
        
        public override UniTask AnimateAsync(ProgressBarAnimationContext ctx)
        {
            var fillImage = ctx.TargetProgressBar.fillImage;
            if (fillImage == null) return UniTask.CompletedTask;

            var originalColor = fillImage.color;
            fillImage.color = FlashColor;

            return LMotion.Create(FlashColor, originalColor, Duration)
                .WithEase(Ease)
                .WithDelay(0.05f)
                .BindToColor(fillImage)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(ProgressBarAnimationContext ctx)
        {
            return UniTask.CompletedTask;
        }
    }
}