using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    [ModuleCategory("Image",Order = 1)] 
    public class ImageFillAmountModule : ButtonAnimationModule
    {
        [Header("Animation Settings")] public Image Target;
        public float From = 0.0f;
        public float To = 1.0f;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutSine;

        public override UniTask AnimateAsync(UIButtonAnimationContext ctx)
        {
            return LMotion.Create(From, To, Duration)
                .WithEase(Ease)
                .BindToFillAmount(Target)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIButtonAnimationContext ctx)
        {
            return LMotion.Create(To, From, Duration)
                .WithEase(Ease)
                .BindToFillAmount(Target)
                .ToUniTask();
        }
    }
}