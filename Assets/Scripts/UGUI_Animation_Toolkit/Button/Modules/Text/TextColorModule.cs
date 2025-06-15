using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    public class TextColorModule : ButtonAnimationModule
    {
        [Header("Animation Settings")] public TextMeshProUGUI Target;
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