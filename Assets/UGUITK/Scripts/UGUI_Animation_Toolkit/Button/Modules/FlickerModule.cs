using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAnimationToolkit.Modules
{
    [Serializable]
    public class FlickerModule : ButtonAnimationModule
    {
        [Header("Flicker Settings")] public Graphic Target;

        [Tooltip("전체 깜빡임 효과가 지속될 시간(초)")] public float Duration = 0.5f;

        [Tooltip("깜빡이는 속도. 값이 작을수록 더 빠르게 깜빡입니다.")]
        public float FlickerInterval = 0.05f;

        [Tooltip("깜빡일때의 Color")] [Range(0, 1)] public Color FlickerColor = Color.red;

        public override async UniTask AnimateAsync(UIButtonAnimationContext ctx)
        {
            // 깜빡임 시작 전의 원래 알파 값을 저장해둡니다.
            var originalColor = Target.color;
            float elapsedTime = 0f;

            try
            {
                // Duration 시간 동안 루프를 돌며 깜빡입니다.
                while (elapsedTime < Duration)
                {
                    Target.color = FlickerColor;
                    await UniTask.Delay(TimeSpan.FromSeconds(FlickerInterval), ignoreTimeScale: true);
                    elapsedTime += FlickerInterval;
                    if (elapsedTime >= Duration) break;

                    Target.color = originalColor;
                    await UniTask.Delay(TimeSpan.FromSeconds(FlickerInterval), ignoreTimeScale: true);
                    elapsedTime += FlickerInterval;
                }
            }
            finally
            {
                // 깜빡임이 끝나면 원래 알파 값으로 복원합니다.
                Target.color = originalColor;
            }
        }

        public override UniTask RevertAsync(UIButtonAnimationContext ctx)
        {
            return UniTask.CompletedTask;
        }
    }
}