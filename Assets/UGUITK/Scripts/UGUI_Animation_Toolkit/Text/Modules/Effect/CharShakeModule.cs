using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace UGUIAnimationToolkit.Text.Modules
{
    [ModuleCategory("Text/Effect", Order = 31)]
    [Serializable]
    public class CharShakeModule : TextAnimationModule
    {
        [Header("Animation Settings")]
        public Vector3 ShakeStrength = Vector3.one * 30f;
        public float ShakeDuration = 1.4f;
        public int Frequency = 7;
        public float DampingRatio = 1f;
        public float StaggerDelay = 0.025f;

        public override UniTask AnimateAsync(TextAnimationContext ctx)
        {
            var textComponent = ctx.TargetText;
            if (textComponent == null) return UniTask.CompletedTask;

            var tasks = new System.Collections.Generic.List<UniTask>();

            for (int i = 0; i < textComponent.textInfo.characterCount; i++)
            {
                var shakeDuration = ShakeDuration * i * StaggerDelay;
                tasks.Add(
                    LMotion.Shake.Create(Vector3.zero, ShakeStrength, shakeDuration)
                        .WithFrequency((int)(shakeDuration / 0.2f))
                        .WithDampingRatio(DampingRatio)
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