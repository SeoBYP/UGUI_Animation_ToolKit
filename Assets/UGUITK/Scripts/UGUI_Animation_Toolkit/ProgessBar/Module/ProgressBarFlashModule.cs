using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAnimationToolkit.ProgressBar.Modules
{
    [ModuleCategory("ProgressBar/Effect", Order = 31)]
    [Serializable]
    public class ProgressBarFlashModule : ProgressBarAnimationModule
    {
        [Header("Animation Settings")]
        [Tooltip("반짝일 때의 색상입니다.")]
        public Color FlashColor = Color.white;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutQuad;

        public override UniTask AnimateAsync(ProgressBarAnimationContext ctx)
        {
            var fillImage = ctx.TargetProgressBar.fillImage;
            if (fillImage == null) return UniTask.CompletedTask;

            var originalColor = fillImage.color;

            // 1. 즉시 FlashColor로 변경
            fillImage.color = FlashColor;

            // 2. 잠시 후 원래 색으로 부드럽게 복귀
            return LMotion.Create(FlashColor, originalColor, Duration)
                .WithEase(Ease)
                .WithDelay(0.05f) // 아주 짧은 딜레이 후 복귀 시작
                .BindToColor(fillImage)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}