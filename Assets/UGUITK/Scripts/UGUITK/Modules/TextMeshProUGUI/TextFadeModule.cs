﻿using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    [ModuleCategory("TextMeshProUGUI",Order = 2)] 
    public class TextFadeModule : UIAnimationModule
    {
        [Header("Animation Settings")] public TextMeshProUGUI Target;
        public float FromAlpha = 1f;
        public float ToAlpha = 0.5f;
        public float Duration = 0.15f;
        public Ease Ease = Ease.InOutSine;

        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            return LMotion.Create(FromAlpha, ToAlpha, Duration)
                .WithEase(Ease)
                .Bind(Target, (a, lbl) => lbl.alpha = a)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            return LMotion.Create(ToAlpha, FromAlpha, Duration)
                .WithEase(Ease)
                .Bind(Target, (a, lbl) => lbl.alpha = a)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}