using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace UGUIAnimationToolkit.Text.Modules
{
    [ModuleCategory("Text/Per-Character", Order = 22)]
    [Serializable]
    public class CharPositionModule : TextAnimationModule
    {
        [Header("Animation Settings")]
        public Vector3 FromOffset = new Vector3(0, -50f, 0);
        public Vector3 ToOffset = Vector3.zero;
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
                    LMotion.Create(FromOffset, ToOffset, CharDuration)
                        .WithEase(Ease)
                        .WithDelay(i * StaggerDelay, skipValuesDuringDelay: SkipValuesDuringDelay)
                        .BindToTMPCharPosition(textComponent, i)
                        .AddTo(ctx.MotionHandle)
                        .ToUniTask()
                );
            }

            return UniTask.WhenAll(tasks);
        }

        public override UniTask RevertAsync(UITextAnimationContext ctx) => UniTask.CompletedTask;
    }
}