using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace UGUIAnimationToolkit.Text.Modules
{
    [ModuleCategory("Text/Per-Character", Order = 21)]
    [Serializable]
    public class CharScaleModule : TextAnimationModule
    {
        [Header("Animation Settings")] public Vector3 From = Vector3.zero;
        public Vector3 To = Vector3.one;
        public float CharDuration = 0.2f;
        public bool SkipValuesDuringDelay = false;
        public float StaggerDelay = 0.05f;
        public Ease Ease = Ease.OutSine;

        public override UniTask AnimateAsync(UITextAnimationContext ctx)
        {
            var textComponent = ctx.TargetText;
            if (textComponent == null) return UniTask.CompletedTask;


            var tasks = new System.Collections.Generic.List<UniTask>();

            for (int i = 0; i < textComponent.textInfo.characterCount; i++)
            {
                tasks.Add(
                    LMotion.Create(From, To, CharDuration)
                        .WithEase(Ease)
                        .WithDelay(i * StaggerDelay,skipValuesDuringDelay:SkipValuesDuringDelay)
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