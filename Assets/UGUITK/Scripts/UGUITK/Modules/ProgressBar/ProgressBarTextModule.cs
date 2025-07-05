using System;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UGUIAnimationToolkit.Core;
using UnityEngine;

namespace UGUIAnimationToolkit.ProgressBar.Modules
{
    [ModuleCategory("ProgressBar/Utility", Order = 100)]
    [Serializable]
    public class ProgressBarTextModule : ProgressBarAnimationModule
    {
        public enum DisplayFormat
        {
            RawValue,
            Percentage,
            Custom
        }

        [Header("Target")] [Tooltip("값을 표시할 TextMeshPro 컴포넌트를 할당하세요.")] [SerializeField]
        private TextMeshProUGUI targetText;

        [Header("Display Settings")] public DisplayFormat Format = DisplayFormat.Percentage;

        [Tooltip("Format이 Custom일 때 사용됩니다. {value}는 현재 값, {max}는 최대값으로 치환됩니다.")]
        public string CustomFormat = "{value} / {max}";

        // UIAnimationContext -> ProgressBarAnimationContext
        public override UniTask AnimateAsync(ProgressBarAnimationContext ctx)
        {
            var targetProgressBar = ctx.TargetProgressBar;
            if (targetText == null || targetProgressBar == null) return UniTask.CompletedTask;

            return LMotion.Create(ctx.StartValue, ctx.TargetValue, 0.3f)
                .Bind(v =>
                {
                    // ... (내부 로직은 동일) ...
                    switch (Format)
                    {
                        case DisplayFormat.RawValue:
                            targetText.text = Mathf.FloorToInt(v).ToString();
                            break;
                        case DisplayFormat.Percentage:
                            var percentage = (v / targetProgressBar.MaxValue) * 100f;
                            targetText.text = $"{Mathf.FloorToInt(percentage)}%";
                            break;
                        case DisplayFormat.Custom:
                            targetText.text = CustomFormat
                                .Replace("{value}", Mathf.FloorToInt(v).ToString())
                                .Replace("{max}", Mathf.FloorToInt(targetProgressBar.MaxValue).ToString());
                            break;
                    }
                })
                .AddTo(ctx.MotionHandle)
                .ToUniTask();
        }

        public override UniTask RevertAsync(ProgressBarAnimationContext ctx) => UniTask.CompletedTask;
    }
}