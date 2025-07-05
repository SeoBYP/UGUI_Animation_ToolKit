using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.Modules.Effects
{
    // 카테고리: "Effect", 순서: 30
    [ModuleCategory("Effect", Order = 30)]
    public class ShakeEffectModule : UIAnimationModule
    {
        [Header("Animation Settings")]
        public Transform Target;
        [Tooltip("흔들리는 강도입니다.")]
        public Vector3 ShakeStrength = new Vector3(10f, 10f, 0f);
        public float Duration = 0.3f;
        public int Frequency = 15;
        public float DampingRatio = 1f;

        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            // [수정] 애니메이션 시작 전, 타겟의 현재 위치를 저장합니다.
            var initialPosition = Target.localPosition;

            // [수정] Vector3.zero 대신, 저장한 현재 위치(initialPosition)를 기준으로 흔들리도록 설정합니다.
            return LMotion.Shake.Create(initialPosition, ShakeStrength, Duration)
                .WithFrequency(Frequency)
                .WithDampingRatio(DampingRatio)
                .BindToLocalPosition(Target)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            return UniTask.CompletedTask;
        }
    }
}