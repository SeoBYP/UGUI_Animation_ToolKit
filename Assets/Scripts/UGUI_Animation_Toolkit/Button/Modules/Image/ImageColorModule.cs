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
    public class ImageColorModule : ButtonAnimationModule
    {
        [Header("Animation Settings")] public Image Target;
        public Color From = Color.white;
        public Color To = Color.gray;
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutSine;

        public override UniTask AnimateAsync(UIButtonAnimationContext ctx)
        {
            return LMotion.Create(From, To, Duration)
                .WithEase(Ease)
                .BindToColor(Target)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIButtonAnimationContext ctx)
        {
            return LMotion.Create(To, From, Duration)
                .WithEase(Ease)
                .BindToColor(Target)
                .ToUniTask();
        }
    }
}