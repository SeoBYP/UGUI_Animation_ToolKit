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
    public class ImageFillAmountModule : UIAnimationModule
    {
        [Header("Animation Settings")] public Image Target;
        public float From = 0.0f;
        public float To = 1.0f;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutSine;

        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            return LMotion.Create(From, To, Duration)
                .WithEase(Ease)
                .BindToFillAmount(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            return LMotion.Create(To, From, Duration)
                .WithEase(Ease)
                .BindToFillAmount(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}