using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.Text.Modules
{
    [ModuleCategory("UGUI Text/Effect", Order = 30)]
    [Serializable]
    public class CharPunchScaleModule : TextAnimationModule // 상속 변경
    {
        [Header("Animation Settings")]
        public Vector3 PunchAmount = Vector3.one * 0.7f;
        public float CharDuration = 1.4f;
        public int Frequency = 7;
        public float DampingRatio = 1f;
        public bool SkipValuesDuringDelay = false;
        public float StaggerDelay = 0.025f;
        public Ease Ease = Ease.OutQuad;

        public override UniTask AnimateAsync(TextAnimationContext ctx) // 시그니처 변경
        {
            var textComponent = ctx.TargetText?.TextMeshProUGUI;
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

        public override UniTask RevertAsync(TextAnimationContext ctx) => UniTask.CompletedTask;
    }
}