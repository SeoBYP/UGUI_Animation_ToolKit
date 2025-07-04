﻿using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    [ModuleCategory("Transform",Order = 3)] 
    public class RotationModule : UIAnimationModule
    {
        [Header("Animation Settings")] public RectTransform Target;
        public Vector3 FromEuler = Vector3.zero;
        public Vector3 ToEuler = new Vector3(0, 0, 15);
        public float Duration = 0.2f;
        public Ease Ease = Ease.OutBack;

        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            return LMotion.Create(
                    Quaternion.Euler(FromEuler),
                    Quaternion.Euler(ToEuler),
                    Duration)
                .WithEase(Ease)
                .BindToLocalRotation(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            return LMotion.Create(
                    Quaternion.Euler(ToEuler),
                    Quaternion.Euler(FromEuler),
                    Duration)
                .WithEase(Ease)
                .BindToLocalRotation(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}