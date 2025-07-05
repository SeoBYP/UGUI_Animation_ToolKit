// =================================================================================
// 파일 경로: Modules/Text/TextRevealModule.cs
// 역할: Animation 1, 2 - 타이핑, 스크램블 효과를 구현합니다.
// =================================================================================

using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UGUIAnimationToolkit.Core;
using Unity.VisualScripting;
using UnityEngine;

namespace UGUIAnimationToolkit.Text.Modules
{
    [ModuleCategory("UGUI Text/Reveal", Order = 10)]
    [Serializable]
    public class TextRevealModule : TextAnimationModule
    {
        [Header("Animation Settings")] [Tooltip("각 문자 텍스트를 표시하는 데 걸리는 시간입니다.")]
        public float CharDuration = 0.05f;

        [Tooltip("Rich Text 태그(<color>, <b> 등)를 지원할지 여부입니다.")]
        public bool SupportRichText = true;

        [Tooltip("타이핑될 때 글자를 랜덤하게 보여주는 스크램블 효과입니다.")]
        public ScrambleMode Scramble = ScrambleMode.None;

        // AnimateAsync 시그니처는 TextAnimationContext를 받습니다.
        public override UniTask AnimateAsync(TextAnimationContext ctx)
        {
            var target = ctx.TargetText;
            if (target == null) return UniTask.CompletedTask;

            // Context에서 시작 텍스트와 목표 텍스트를 가져와 애니메이션을 생성합니다.
            var builder = LMotion.String.Create512Bytes(ctx.StartText, ctx.TargetTextValue, CharDuration)
                .WithScrambleChars(Scramble);

            if (SupportRichText)
            {
                builder = builder.WithRichText();
            }

            return builder.BindToText(target.TextMeshProUGUI)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(TextAnimationContext ctx)
        {
            // 되돌리기는 즉시 목표 텍스트(최종 텍스트)로 설정합니다.
            var target = ctx.TargetText;
            if (target == null) return UniTask.CompletedTask;
            target.TextMeshProUGUI.text = ctx.TargetTextValue;
            return UniTask.CompletedTask;
        }
    }
}