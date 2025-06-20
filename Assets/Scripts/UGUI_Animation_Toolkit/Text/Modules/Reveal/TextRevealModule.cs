// =================================================================================
// 파일 경로: Modules/Text/TextRevealModule.cs
// 역할: Animation 1, 2 - 타이핑, 스크램블 효과를 구현합니다.
// =================================================================================

using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace UGUIAnimationToolkit.Text.Modules
{
    [ModuleCategory("Text/Reveal", Order = 10)]
    [Serializable]
    public class TextRevealModule : TextAnimationModule
    {
        [Header("Animation Settings")] 
        //public string ReavealText = "Grumpy wizards make toxic brew for the evil Queen and Jack.";
        [Tooltip("각 문자 텍스트를 표시하는 데 걸리는 시간입니다.")]
        public float CharDuration = 0.05f;
        [Tooltip("Rich Text 태그(<color>, <b> 등)를 지원할지 여부입니다.")]
        public bool SupportRichText = true;
        [Tooltip("타이핑될 때 글자를 랜덤하게 보여주는 스크램블 효과입니다.")]
        public ScrambleMode Scramble = ScrambleMode.None;

        private string _initialText;

        public override UniTask AnimateAsync(UITextAnimationContext ctx)
        {
            var textComponent = ctx.TargetText;
            if (textComponent == null) return UniTask.CompletedTask;
            
            _initialText = textComponent.text;

            var builder = LMotion.String.Create512Bytes(string.Empty, _initialText, CharDuration * _initialText.Length)
                .WithScrambleChars(Scramble);

            if (SupportRichText)
            {
                builder = builder.WithRichText();
            }

            return builder.BindToText(textComponent)
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(UITextAnimationContext ctx)
        {
            if (ctx.TargetText == null) return UniTask.CompletedTask;
            // 되돌리기는 즉시 원래 텍스트로 설정합니다.
            ctx.TargetText.text = _initialText;
            return UniTask.CompletedTask;
        }
    }
}