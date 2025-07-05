using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.Text.Modules
{
    [ModuleCategory("UGUI Text/Per-Character", Order = 20)] // 카테고리 추가
    [Serializable]
    public class CharRotateInModule : TextAnimationModule // 상속 변경
    {
        public enum Axis { X, Y, Z }

        [Header("Animation Settings")]
        public Axis RotationAxis = Axis.Y;
        public float FromAngle = -90f;
        public float ToAngle = 0f;
        public float CharDuration = 0.25f;
        public bool SkipValuesDuringDelay = false;
        public float StaggerDelay = 0.05f;
        public Ease Ease = Ease.OutBack;
        
        public override UniTask AnimateAsync(TextAnimationContext ctx) // 시그니처 변경
        {
            var textComponent = ctx.TargetText?.TextMeshProUGUI;
            if (textComponent == null) return UniTask.CompletedTask;

            var tasks = new List<UniTask>();

            for (int i = 0; i < textComponent.textInfo.characterCount; i++)
            {
                var tcs = new UniTaskCompletionSource();

                var motion = LMotion.Create(FromAngle, ToAngle, CharDuration)
                    .WithEase(Ease)
                    .WithDelay(i * StaggerDelay, skipValuesDuringDelay: SkipValuesDuringDelay)
                    .WithOnComplete(() => tcs.TrySetResult());

                switch (RotationAxis)
                {
                    case Axis.X: motion.BindToTMPCharEulerAnglesX(textComponent, i).AddTo(ctx.MotionHandle); break;
                    case Axis.Y: motion.BindToTMPCharEulerAnglesY(textComponent, i).AddTo(ctx.MotionHandle); break;
                    case Axis.Z: motion.BindToTMPCharEulerAnglesZ(textComponent, i).AddTo(ctx.MotionHandle); break;
                }
                tasks.Add(tcs.Task);
            }

            return UniTask.WhenAll(tasks);
        }

        public override UniTask RevertAsync(TextAnimationContext ctx) => UniTask.CompletedTask;
    }
}