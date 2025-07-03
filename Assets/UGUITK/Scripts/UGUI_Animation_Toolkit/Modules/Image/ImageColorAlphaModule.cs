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
    public class ImageColorAlphaModule : UIAnimationModule
    {
        [Header("Animation Settings")] 
        public Image Target;
        public float From = 0;
        public float To = 1;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutSine;
        
        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            // [개선] return 키워드 추가 및 AddTo로 모션 핸들링
            return LMotion.Create(From, To, Duration)
                .WithEase(Ease)
                .BindToColorA(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            return LMotion.Create(To, From, Duration)
                .WithEase(Ease)
                .BindToColorA(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}