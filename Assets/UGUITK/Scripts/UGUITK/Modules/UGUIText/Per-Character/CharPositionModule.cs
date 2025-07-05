using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.Text.Modules
{
    [ModuleCategory("UGUI Text/Per-Character", Order = 22)]
    [Serializable]
    public class CharPositionModule : TextAnimationModule // 상속 변경
    {
        [Header("Animation Settings")]
        public Vector3 FromOffset = new Vector3(0, -50f, 0);
        public Vector3 ToOffset = Vector3.zero;
        public float CharDuration = 0.2f;
        public bool SkipValuesDuringDelay = false;
        public float StaggerDelay = 0.05f;
        public Ease Ease = Ease.OutSine;

        public override UniTask AnimateAsync(TextAnimationContext ctx) // 시그니처 변경
        {
            var textComponent = ctx.TargetText?.TextMeshProUGUI;
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

        public override UniTask RevertAsync(TextAnimationContext ctx) => UniTask.CompletedTask;
    }
}