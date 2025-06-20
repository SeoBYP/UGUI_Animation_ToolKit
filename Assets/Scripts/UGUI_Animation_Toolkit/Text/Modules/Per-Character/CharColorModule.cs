using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace UGUIAnimationToolkit.Text.Modules
{
    [ModuleCategory("Text/Per-Character", Order = 23)]
    [Serializable]
    public class CharColorModule : TextAnimationModule
    {
        [Header("Animation Settings")] public Color From = Color.white;
        public Color To = Color.red;
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
                        .WithDelay(i * StaggerDelay, skipValuesDuringDelay: SkipValuesDuringDelay)
                        .BindToTMPCharColor(textComponent, i)
                        .AddTo(ctx.MotionHandle)
                        .ToUniTask()
                );
            }

            return UniTask.WhenAll(tasks);
        }

        public override UniTask RevertAsync(UITextAnimationContext ctx) => UniTask.CompletedTask;
    }
}