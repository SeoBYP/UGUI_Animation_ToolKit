using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    public class TextFadeModule : ButtonAnimationModule
    {
        public float FromAlpha = 1f;
        public float ToAlpha = 0.5f;
        public float Duration = 0.15f;
        public Ease Ease = Ease.InOutSine;

        public override UniTask AnimateAsync(UIButtonAnimationContext ctx)
        {
            return LMotion.Create(FromAlpha, ToAlpha, Duration)
                .WithEase(Ease)
                .Bind(ctx.Label, (a, lbl) => lbl.alpha = a)
                .ToUniTask();
        }
    }
}