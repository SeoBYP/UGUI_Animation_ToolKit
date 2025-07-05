using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAnimationToolkit.Modules.Effects
{
    // 카테고리: "Effect", 순서: 30
    [ModuleCategory("Effect", Order = 30)]
    [Serializable]
    public class RippleEffectModule : UIAnimationModule
    {
        [Header("Animation Settings")] [Tooltip("리플 효과를 위한 별도의 Image 컴포넌트를 할당하세요.")] [SerializeField]
        private Image rippleImage;

        [Header("Animation Settings")] public float FillDuration = 0.15f;
        public float FadeOutDuration = 0.25f;
        public Ease EaseType = Ease.OutSine;

        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            if (ctx.PointerEventData is null)
            {
                Debug.Log("PointerEventData is null.");
                return UniTask.CompletedTask;
            }
            // 마우스 클릭 위치로 리플 이미지 이동
            rippleImage.rectTransform.position = ctx.PointerEventData.position;

            // 알파와 스케일을 동시에 애니메이션
            var fillTask = LMotion.Create(0f, 1f, FillDuration)
                .WithEase(EaseType)
                .BindToColorA(rippleImage)
                .AddTo(ctx.MotionHandle);

            var scaleTask = LMotion.Create(Vector3.zero, Vector3.one, FillDuration)
                .WithEase(EaseType)
                .BindToLocalScale(rippleImage.transform)
                .AddTo(ctx.MotionHandle);

            return UniTask.WhenAll(fillTask.ToUniTask(), scaleTask.ToUniTask());
        }

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            // Up 이벤트에서는 알파 값만 서서히 사라지게 합니다.
            return LMotion.Create(rippleImage.color.a, 0f, FadeOutDuration)
                .WithEase(EaseType)
                .BindToColorA(rippleImage)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}