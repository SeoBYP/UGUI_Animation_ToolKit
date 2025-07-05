using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAnimationToolkit.ProgressBar.Modules
{
    // 카테고리도 더 범용적인 이름으로 수정합니다.
    [ModuleCategory("ProgressBar/Effect", Order = 20)]
    [Serializable]
    public class DelayedFillModule : ProgressBarAnimationModule
    {
        [Header("Target Graphics")]
        [Tooltip("값의 변화를 지연해서 따라올 배경 그래픽입니다. (Image Type이 Filled여야 합니다)")]
        [SerializeField] private Image m_DelayedFillImage;

        [Header("Animation Settings")]
        [Tooltip("지연 애니메이션이 시작되기 전의 대기 시간입니다.")]
        [SerializeField] private float m_Delay = 0.3f;
        [Tooltip("지연 애니메이션의 지속 시간입니다.")]
        [SerializeField] private float m_Duration = 0.5f;
        [SerializeField] private Ease m_Ease = Ease.OutCubic;

        public override UniTask AnimateAsync(ProgressBarAnimationContext ctx)
        {
            var targetProgressBar = ctx.TargetProgressBar;
            if (targetProgressBar == null || m_DelayedFillImage == null) return UniTask.CompletedTask;
            
            // 정규화된 값으로 변환
            var startNormalized = ctx.StartValue / targetProgressBar.MaxValue;
            var targetNormalized = ctx.TargetValue / targetProgressBar.MaxValue;

            if (ctx.TargetValue < ctx.StartValue) 
            {
                m_DelayedFillImage.fillAmount = startNormalized;
                return LMotion.Create(startNormalized, targetNormalized, m_Duration)
                    .WithEase(m_Ease)
                    .WithDelay(m_Delay)
                    .BindToFillAmount(m_DelayedFillImage)
                    .AddTo(ctx.MotionHandle)
                    .ToUniTask();
            }
            else
            {
                m_DelayedFillImage.fillAmount = targetNormalized;
            }
            return UniTask.CompletedTask;
        }

        public override UniTask RevertAsync(ProgressBarAnimationContext ctx)
        {
            return UniTask.CompletedTask;
        }
    }
}