using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    [ModuleCategory("Text",Order = 2)] 
    public class TextColorModule : UIAnimationModule
    {
        [Header("Animation Settings")] public TextMeshProUGUI Target;
        public Color From = Color.white;
        public Color To = Color.gray;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutSine;
        
        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            return LMotion.Create(From, To, Duration)
                .WithEase(Ease)
                .BindToColor(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            return LMotion.Create(To, From, Duration)
                .WithEase(Ease)
                .BindToColor(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}