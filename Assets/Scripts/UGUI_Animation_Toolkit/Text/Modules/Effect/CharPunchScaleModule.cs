using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace UGUIAnimationToolkit.Text.Modules
{
    [ModuleCategory("Text/Effect", Order = 30)]
    [Serializable]
    public class CharPunchScaleModule : TextAnimationModule
    {
        [Header("Animation Settings")] public Vector3 PunchAmount = Vector3.one * 0.7f;
        public float CharDuration = 1.4f;
        public int Frequency = 7;
        public float DampingRatio = 1f;
        public bool SkipValuesDuringDelay = false;
        public float StaggerDelay = 0.025f;
        public Ease Ease = Ease.OutQuad;

        public override UniTask AnimateAsync(UITextAnimationContext ctx)
        {
            var textComponent = ctx.TargetText;
            if (textComponent == null) return UniTask.CompletedTask;

            var tasks = new System.Collections.Generic.List<UniTask>();

            for (int i = 0; i < textComponent.textInfo.characterCount; i++)
            {
                tasks.Add(
                    LMotion.Punch.Create(Vector3.one, PunchAmount, CharDuration)
                        .WithEase(Ease)
                        .WithDelay(i * StaggerDelay, skipValuesDuringDelay: SkipValuesDuringDelay)
                        .WithFrequency(Frequency)
                        .WithDampingRatio(DampingRatio)
                        .BindToTMPCharScale(textComponent, i)
                        .AddTo(ctx.MotionHandle)
                        .ToUniTask()
                );
            }

            return UniTask.WhenAll(tasks);
        }

        public override UniTask RevertAsync(UITextAnimationContext ctx) => UniTask.CompletedTask;
    }
}