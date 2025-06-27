using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
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
            var mainFillImage = ctx.TargetProgressBar.fillImage;
            if (mainFillImage == null || m_DelayedFillImage == null) return UniTask.CompletedTask;
            
            // 데미지를 입었는지(값이 감소했는지) 확인
            if (ctx.TargetValue < ctx.StartValue) 
            {
                // 지연 바의 시작 값은 메인 바의 시작 값과 동일합니다.
                m_DelayedFillImage.fillAmount = ctx.StartValue;
                
                // 메인 바는 ProgressBarValueModule이 애니메이션하므로,
                // 여기서는 지연 바만 지정된 딜레이 이후에 목표 값까지 애니메이션합니다.
                return LMotion.Create(ctx.StartValue, ctx.TargetValue, m_Duration)
                    .WithEase(m_Ease)
                    .WithDelay(m_Delay) // 메인 바가 줄어든 후 딜레이
                    .BindToFillAmount(m_DelayedFillImage)
                    .AddTo(ctx.MotionHandle)
                    .ToUniTask();
            }
            else // 회복한 경우
            {
                // 회복 시에는 지연 바가 즉시 목표 값으로 이동하여 메인 바 뒤에 깔끔하게 위치합니다.
                m_DelayedFillImage.fillAmount = ctx.TargetValue;
            }

            return UniTask.CompletedTask;
        }
    }
}