using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.ProgressBar.Modules
{
    [ModuleCategory("ProgressBar/Effect", Order = 30)]
    [Serializable]
    public class ProgressBarShakeModule : ProgressBarAnimationModule
    {
        [Header("Animation Settings")]
        public UGUIProgressBar TargetProgressBar;
        [Tooltip("흔들리는 강도입니다.")]
        public Vector3 ShakeStrength = new Vector3(10f, 10f, 0f);
        public float Duration = 0.3f;
        public int Frequency = 15;
        public float DampingRatio = 1f;

        public override UniTask AnimateAsync(ProgressBarAnimationContext ctx)
        {
            var target = ctx.TargetProgressBar.transform;
            if (target == null) return UniTask.CompletedTask;

            // 값 감소(데미지) 시에만 흔들림 효과를 적용합니다.
            if (ctx.TargetValue < ctx.StartValue)
            {
                // [수정] 애니메이션 시작 전, 타겟의 현재 위치를 저장합니다.
                var initialPosition = target.localPosition;

                // [수정] Vector3.zero 대신, 저장한 현재 위치(initialPosition)를 기준으로 흔들리도록 설정합니다.
                return LMotion.Shake.Create(initialPosition, ShakeStrength, Duration)
                    .WithFrequency(Frequency)
                    .WithDampingRatio(DampingRatio)
                    .BindToLocalPosition(target)
                    .AddTo(ctx.MotionHandle)
                    .ToUniTask();
            }

            return UniTask.CompletedTask;
        }

        public override UniTask RevertAsync(ProgressBarAnimationContext ctx)
        {
            return UniTask.CompletedTask;
        }
    }
}