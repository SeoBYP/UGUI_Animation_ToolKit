using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using TMPro;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.Editor
{
    [Serializable]
    [ModuleCategory("Text", Order = 2)]
    public class TextCharacterSpacingModule : UIAnimationModule
    {
        [Header("Target")] [Tooltip("자간을 변경할 TextMeshPro 컴포넌트를 할당하세요.")] [SerializeField]
        private TextMeshProUGUI targetText;

        [Header("Animation Settings")] public float To = 12f;
        public float Duration = 0.25f;
        public Ease Ease = Ease.OutSine;

        // 애니메이션 시작 전의 초기 자간 값을 기억할 변수
        private float _initialSpacing;
        
        public override UniTask AnimateAsync(UIAnimationContext ctx)
        {
            // 애니메이션 시작 직전의 현재 자간 값을 저장
            _initialSpacing = targetText.characterSpacing;

            // LitMotion으로 자간(characterSpacing) 애니메이션 생성
            return LMotion.Create(_initialSpacing, To, Duration)
                .WithEase(Ease)
                .Bind(x => targetText.characterSpacing = x) // Bind를 사용하여 값 직접 할당
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UIAnimationContext ctx)
        {
            // 현재 자간 값에서 저장해두었던 초기 값(_initialSpacing)으로 되돌림
            return LMotion.Create(targetText.characterSpacing, _initialSpacing, Duration)
                .WithEase(Ease)
                .Bind(x => targetText.characterSpacing = x)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }
    }
}