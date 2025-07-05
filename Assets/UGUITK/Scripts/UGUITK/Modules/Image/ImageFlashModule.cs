using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    [ModuleCategory("Image", Order = 1)]
    public class ImageFlashModule : UIAnimationModule
    {
        [Header("Animation Settings")] public Image Target;
        public Color FlashColor = Color.white;
        public float Duration = 0.2f;
        public float Delay = 0.05f;
        public Ease Ease = Ease.OutQuad;

        private Color originalColor;
        
        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            originalColor = Target.color;
            // 2. 잠시 후 원래 색으로 부드럽게 복귀
            return LMotion.Create(originalColor, FlashColor, Duration)
                .WithEase(Ease)
                .WithDelay(Delay) // 아주 짧은 딜레이 후 복귀 시작
                .BindToColor(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            return LMotion.Create(FlashColor, originalColor, Duration)
                .WithEase(Ease)
                .WithDelay(Delay) // 아주 짧은 딜레이 후 복귀 시작
                .BindToColor(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}