using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIAnimationToolkit.Modules
{
    [ModuleCategory("Effect", Order = 30)]
    [Serializable]
    public class PulseEffectModule : UIAnimationModule
    {
        [Header("Target")] [Tooltip("펄스 효과를 위한 별도의 Image 컴포넌트를 할당하세요.")] [SerializeField]
        private Image pulseImage;

        [Header("Animation Settings")] [Tooltip("펄스가 커지는 크기입니다.")]
        public float PulseSize = 40f;

        public float Duration = 0.2f;
        public Ease Ease = Ease.OutSine;

        // 펄스 효과 시작 전의 초기 상태를 기억할 변수들
        private Vector2 _initialSize;
        private Color _initialColor;

        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            // 펄스 애니메이션 시작 전에 이미지의 상태를 초기화합니다.
            // 이렇게 해야 마우스를 올릴 때마다 항상 같은 효과가 나타납니다.
            if (_initialSize == Vector2.zero) // 최초 실행 시에만 초기값 저장
            {
                _initialSize = pulseImage.rectTransform.sizeDelta;
                _initialColor = pulseImage.color;
            }
            else // 두 번째 실행부터는 저장된 초기값으로 리셋
            {
                pulseImage.rectTransform.sizeDelta = _initialSize;
                pulseImage.color = _initialColor;
            }

            // 사이즈와 알파(투명도) 애니메이션을 동시에 실행합니다.
            var sizeTask = LMotion.Create(_initialSize, _initialSize + new Vector2(PulseSize, PulseSize), Duration)
                .WithEase(Ease)
                .BindToSizeDelta(pulseImage.rectTransform)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();

            var alphaTask = LMotion.Create(_initialColor.a, 0f, Duration)
                .WithEase(Ease)
                .BindToColorA(pulseImage)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();

            // 두 애니메이션이 모두 끝날 때까지 기다립니다.
            return UniTask.WhenAll(sizeTask, alphaTask)
                .ContinueWith(() =>
                {
                    pulseImage.rectTransform.sizeDelta = _initialSize;
                    pulseImage.color = _initialColor;
                });
        }

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            return UniTask.CompletedTask;
        }
    }
}