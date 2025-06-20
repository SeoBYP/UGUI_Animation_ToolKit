using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Text.Modules
{
    [ModuleCategory("Text/Per-Character", Order = 20)]
    [Serializable]
    public class CharRotateInModule : TextAnimationModule
    {
        public enum Axis
        {
            X,
            Y,
            Z
        }

        [Header("Animation Settings")] public Axis RotationAxis = Axis.Y;
        public float FromAngle = -90f;
        public float ToAngle = 0f;
        public float CharDuration = 0.25f;
        public bool SkipValuesDuringDelay = false;
        public float StaggerDelay = 0.05f;
        public Ease Ease = Ease.OutBack;

        public override UniTask AnimateAsync(UITextAnimationContext ctx)
        {
            var textComponent = ctx.TargetText;
            if (textComponent == null) return UniTask.CompletedTask;


            var tasks = new List<UniTask>();

            for (int i = 0; i < textComponent.textInfo.characterCount; i++)
            {
                var tcs = new UniTaskCompletionSource();

                var motion = LMotion.Create(FromAngle, ToAngle, CharDuration)
                    .WithEase(Ease)
                    .WithDelay(i * StaggerDelay, skipValuesDuringDelay: SkipValuesDuringDelay)
                    .WithOnComplete(() => tcs.TrySetResult()); // 수정

                switch (RotationAxis)
                {
                    case Axis.X:
                        motion.BindToTMPCharEulerAnglesX(textComponent, i)
                            .AddTo(ctx.MotionHandle);
                        break;
                    case Axis.Y:
                        motion.BindToTMPCharEulerAnglesY(textComponent, i)
                            .AddTo(ctx.MotionHandle);
                        break;
                    case Axis.Z:
                        motion.BindToTMPCharEulerAnglesZ(textComponent, i)
                            .AddTo(ctx.MotionHandle);
                        break;
                }

                tasks.Add(tcs.Task);
            }

            return UniTask.WhenAll(tasks);
        }

        public override UniTask RevertAsync(UITextAnimationContext ctx)
        {
            return UniTask.CompletedTask;
        }
    }
}